using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdStatistic
{
    public partial class MainWindow : Window
    {
        private CampaignInput _lastCampaignInput;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void CalculateEfficiencyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryReadCampaignInput(out var input, out var errorMessage))
            {
                EfficiencyResultTextBlock.Text = $"❌ Ошибка ввода: {errorMessage}";
                return;
            }

            _lastCampaignInput = input;

            var costPerThousandViews = input.Budget / input.Views * 1000;
            var costPerClick = input.Budget / input.Clicks;
            var clickShare = input.Clicks / input.Views * 100;
            var customersGrowthPercent = (input.CustomersAfterPerDay - input.CustomersBeforePerDay) / input.CustomersBeforePerDay * 100;
            var extraCustomersPerDay = input.CustomersAfterPerDay - input.CustomersBeforePerDay;
            var extraProfitPerDay = extraCustomersPerDay * input.ProfitPerCustomer;
            var netDailyResult = extraProfitPerDay - input.Budget;
            var profitabilityIndex = input.Budget == 0 ? 0 : (extraProfitPerDay / input.Budget) * 100;
            var paybackDays = extraProfitPerDay > 0 ? input.Budget / extraProfitPerDay : double.PositiveInfinity;

            var verdict = extraProfitPerDay >= input.Budget
                ? "✅ Кампания выглядит выгодной: прирост клиентов покрывает бюджет за 1 день или быстрее."
                : "⚠️ Кампания окупается дольше 1 дня — стоит оптимизировать креатив или аудиторию.";

            EfficiencyResultTextBlock.Text =
                $"• За 1000 просмотров вы платите: {costPerThousandViews:N2} ₽\n" +
                $"• Стоимость одного клика: {costPerClick:N2} ₽\n" +
                $"• Доля кликов от просмотров: {clickShare:N2}%\n" +
                $"• Рост клиентов: {customersGrowthPercent:N2}% ({extraCustomersPerDay:N1} доп. клиента/день)\n" +
                $"• Дополнительная прибыль в день: {extraProfitPerDay:N2} ₽\n" +
                $"• Соотношение прибыль/затраты за день: {profitabilityIndex:N1}%\n" +
                $"• Прогноз окупаемости: {(double.IsPositiveInfinity(paybackDays) ? "нет окупаемости" : paybackDays.ToString("N1") + " дня")}\n" +
                $"• Чистый результат первого дня: {netDailyResult:N2} ₽\n\n" +
                verdict;

            ForecastResultTextBlock.Text = "Теперь введите новый бюджет и нажмите «Построить прогноз».";
        }

        private void CalculateForecastButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastCampaignInput == null)
            {
                ForecastResultTextBlock.Text = "⚠️ Сначала рассчитайте текущую эффективность кампании.";
                return;
            }

            if (!TryReadDouble(ForecastBudgetTextBox.Text, out var newBudget) || newBudget <= 0)
            {
                ForecastResultTextBlock.Text = "❌ Укажите корректный новый бюджет (число больше 0).";
                return;
            }

            var scale = newBudget / _lastCampaignInput.Budget;
            var predictedViews = _lastCampaignInput.Views * scale;
            var predictedClicks = _lastCampaignInput.Clicks * scale;
            var extraCustomers = (_lastCampaignInput.CustomersAfterPerDay - _lastCampaignInput.CustomersBeforePerDay) * scale;
            var predictedCustomersPerDay = _lastCampaignInput.CustomersBeforePerDay + extraCustomers;
            var predictedExtraProfitPerDay = extraCustomers * _lastCampaignInput.ProfitPerCustomer;
            var paybackDays = predictedExtraProfitPerDay > 0 ? newBudget / predictedExtraProfitPerDay : double.PositiveInfinity;

            ForecastResultTextBlock.Text =
                $"При бюджете {newBudget:N0} ₽ (в {scale:N2} раза от текущего) прогноз такой:\n\n" +
                $"• Просмотры: {predictedViews:N0}\n" +
                $"• Клики: {predictedClicks:N0}\n" +
                $"• Клиентов в день: {predictedCustomersPerDay:N1}\n" +
                $"• Дополнительная прибыль в день: {predictedExtraProfitPerDay:N2} ₽\n" +
                $"• Окупаемость: {(double.IsPositiveInfinity(paybackDays) ? "не прогнозируется" : paybackDays.ToString("N1") + " дня")}\n\n" +
                "ℹ️ Прогноз линейный: предполагается, что эффективность рекламы сохраняется при изменении бюджета.";
        }

        private bool TryReadCampaignInput(out CampaignInput input, out string errorMessage)
        {
            input = null;
            errorMessage = string.Empty;

            if (!TryReadDouble(BudgetTextBox.Text, out var budget) || budget <= 0)
            {
                errorMessage = "бюджет кампании должен быть больше 0";
                return false;
            }

            if (!TryReadDouble(HoursTextBox.Text, out var hours) || hours <= 0)
            {
                errorMessage = "количество часов должно быть больше 0";
                return false;
            }

            if (!TryReadDouble(ViewsTextBox.Text, out var views) || views <= 0)
            {
                errorMessage = "количество просмотров должно быть больше 0";
                return false;
            }

            if (!TryReadDouble(ClicksTextBox.Text, out var clicks) || clicks <= 0)
            {
                errorMessage = "количество кликов должно быть больше 0";
                return false;
            }

            if (!TryReadDouble(CustomersBeforeTextBox.Text, out var customersBefore) || customersBefore <= 0)
            {
                errorMessage = "обычное число клиентов в день должно быть больше 0";
                return false;
            }

            if (!TryReadDouble(CustomersAfterTextBox.Text, out var customersAfter) || customersAfter <= 0)
            {
                errorMessage = "число клиентов после рекламы должно быть больше 0";
                return false;
            }

            if (!TryReadDouble(ProfitPerCustomerTextBox.Text, out var profitPerCustomer) || profitPerCustomer < 0)
            {
                errorMessage = "прибыль с клиента не может быть отрицательной";
                return false;
            }

            input = new CampaignInput
            {
                Budget = budget,
                Hours = hours,
                Views = views,
                Clicks = clicks,
                CustomersBeforePerDay = customersBefore,
                CustomersAfterPerDay = customersAfter,
                ProfitPerCustomer = profitPerCustomer
            };

            return true;
        }

        private static bool TryReadDouble(string source, out double result)
        {
            var normalized = source?.Trim().Replace(',', '.') ?? string.Empty;
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private class CampaignInput
        {
            public double Budget { get; set; }
            public double Hours { get; set; }
            public double Views { get; set; }
            public double Clicks { get; set; }
            public double CustomersBeforePerDay { get; set; }
            public double CustomersAfterPerDay { get; set; }
            public double ProfitPerCustomer { get; set; }
        }
    }
}
