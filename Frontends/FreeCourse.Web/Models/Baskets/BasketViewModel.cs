using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Web.Models.Baskets
{
    public class BasketViewModel
    {
        public string UserId { get; set; }
        public string DiscountCode { get; set; }

        //nece faiz endrim olub onu saxlayirig.
        public int? DiscountRate { get; set; }
        private List<BasketItemViewModel> _basketItems { get; set; }

        public List<BasketItemViewModel> BasketItems
        {
            get
            {
                if (HasDiscount)
                {
                    //Example => kursun qiymeti 100 manatdir endrim ise 10 faizdir
                    _basketItems.ForEach(x =>
                    {
                        //100 manatin 10 faizin hesablayirig =>10 manat
                        var discountPrice = x.Price * ((decimal)DiscountRate.Value / 100);

                        //daha sonra qiymetden endirim yani 10 manati cixirig => 90 manat
                        //sondaki iki yuvarlasdirmag ucundur yani vergulden sora 2 xarekter olsun
                        x.AppliedDiscount(Math.Round(x.Price - discountPrice, 2));
                    });
                }
                return _basketItems;
            }
            set { _basketItems = value; }
        }

        //Methods
        public decimal TotalPrice
        {
            get => _basketItems.Sum(x => x.GetCurrentPrice);
        }

        public bool HasDiscount
        {
            //discountCode var ise true yox ise false gaytaracag
            get => !string.IsNullOrEmpty(DiscountCode) && DiscountRate.HasValue;
        }

        public void CancelDiscount()
        {
            DiscountCode = null;
            DiscountRate = null;
        }

        public void ApplyDiscount(string code, int rate)
        {
            DiscountCode = code;
            DiscountRate = rate;
        }
    }
}