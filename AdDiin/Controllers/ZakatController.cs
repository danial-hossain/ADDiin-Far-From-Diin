using AdDiin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class ZakatController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new ZakatCalculatorViewModel
            {
                GoldPrice = 8500,
                SilverPrice = 120
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calculate(ZakatCalculatorViewModel model)
        {
            var cash = Math.Max(0, model.Cash);
            var goldVal = Math.Max(0, model.GoldWeight) * Math.Max(0, model.GoldPrice);
            var silverVal = Math.Max(0, model.SilverWeight) * Math.Max(0, model.SilverPrice);
            var invVal = Math.Max(0, model.Investments);
            var inventoryVal = Math.Max(0, model.BusinessInventory);
            var otherVal = Math.Max(0, model.OtherAssets);
            var debtVal = Math.Max(0, model.Debts);

            var totalAssets = cash + goldVal + silverVal + invVal + inventoryVal + otherVal;
            var net = Math.Max(0, totalAssets - debtVal);

            var nisabGold = 87.48m * model.GoldPrice;
            var nisabSilver = 612.36m * model.SilverPrice;
            var threshold = (nisabSilver > 0 && (nisabGold == 0 || nisabSilver < nisabGold)) ? nisabSilver : nisabGold;

            var isEligible = net >= threshold && threshold > 0;
            var zakat = isEligible ? net * 0.025m : 0;

            model.TotalAssets = totalAssets;
            model.NetWealth = net;
            model.NisabGoldThreshold = nisabGold;
            model.NisabSilverThreshold = nisabSilver;
            model.NisabThresholdUsed = threshold;
            model.IsEligible = isEligible;
            model.ZakatPayable = zakat;
            model.HasCalculated = true;

            return View("Index", model);
        }
    }
}
