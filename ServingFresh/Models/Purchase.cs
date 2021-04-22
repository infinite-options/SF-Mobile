using System;
using System.Collections.ObjectModel;

namespace ServingFresh.Models
{
    public class Purchase
    {
        private string pur_customer_uid;
        private string pur_business_uid;
        private ObservableCollection<PurchasedItem> items;
        private string order_instructions;
        private string delivery_instructions;
        private string order_type;
        private string delivery_first_name;
        private string delivery_last_name;
        private string delivery_phone_num;
        private string delivery_email;
        private string delivery_address;
        private string delivery_unit;
        private string delivery_city;
        private string delivery_state;
        private string delivery_zip;
        private string delivery_latitude;
        private string delivery_longitude;
        private string purchase_notes;
        private string start_delivery_date;
        private string pay_coupon_id;
        private string amount_due;
        private string amount_discount;
        private string amount_paid;
        private string info_is_Addon;
        private string cc_num;
        private string cc_exp_date;
        private string cc_cvv;
        private string cc_zip;
        private string charge_id;
        private string payment_type;
        private string subtotal;
        private string service_fee;
        private string delivery_fee;
        private string driver_tip;
        private string taxes;

        public Purchase(User user)
        {
            pur_customer_uid = user.getUserID();
            pur_business_uid = "";
            items = null;
            order_instructions = "";
            delivery_instructions = "";
            order_type = "";
            delivery_first_name = user.getUserFirstName();
            delivery_last_name = user.getUserLastName();
            delivery_phone_num = user.getUserPhoneNumber();
            delivery_email = user.getUserEmail();
            delivery_address = user.getUserAddress();
            delivery_unit = user.getUserUnit();
            delivery_city = user.getUserCity();
            delivery_state = user.getUserState();
            delivery_zip = user.getUserZipcode();
            delivery_latitude = user.getUserLatitude();
            delivery_longitude = user.getUserLongitude();
            purchase_notes = "purchase_notes";
            start_delivery_date = "";
            pay_coupon_id = "";
            amount_due = "";
            amount_discount = "";
            amount_paid = "";
            info_is_Addon = "";
            cc_num = "";
            cc_exp_date = "";
            cc_cvv = "";
            cc_zip = "";
            charge_id = "";
            payment_type = "";
            subtotal = "";
            service_fee = "";
            delivery_fee = "";
            driver_tip = "";
            taxes = "";
        }

        public string getPurchaseCustomerUID()
        {
            return pur_customer_uid;
        }

        public string getPurchaseBusinessUID()
        {
            return pur_business_uid;
        }

        public ObservableCollection<PurchasedItem> getPurchaseItems()
        {
            return items;
        }

        public string getPurchaseInstructions()
        {
            return order_instructions;
        }

        public string getPurchaseDeliveryInstructions()
        {
            return delivery_instructions;
        }

        public string getPurchaseOrderType()
        {
            return order_type;
        }

        public string getPurchaseFirstName()
        {
            return delivery_first_name;
        }

        public string getPurchaseLastName()
        {
            return delivery_last_name;
        }

        public string getPurchasePhoneNumber()
        {
            return delivery_phone_num;
        }

        public string getPurchaseEmail()
        {
            return delivery_email;
        }

        public string getPurchaseAddress()
        {
            return delivery_address;
        }

        public string getPurchaseUnit()
        {
            return delivery_unit;
        }

        public string getPurchaseCity()
        {
            return delivery_city;
        }

        public string getPurchaseState()
        {
            return delivery_state;
        }

        public string getPurchaseZipcode()
        {
            return delivery_zip;
        }

        public string getPurchaseLatitude()
        {
            return delivery_latitude;
        }

        public string getPurchaseLongitude()
        {
            return delivery_longitude;
        }

        public string getPurchaseNotes()
        {
            return purchase_notes;
        }

        public string getPurchaseDeliveryDate()
        {
            return start_delivery_date;
        }

        public string getPurchaseCoupoID()
        {
            return pay_coupon_id;
        }

        public string getPurchaseAmountDue()
        {
            return amount_due;
        }

        public string getPurchaseDiscount()
        {
            return amount_discount;
        }

        public string getPurchasePaid()
        {
            return amount_paid;
        }

        public string getPurchaseAddon()
        {
            return info_is_Addon;
        }

        public string getPurchaseCCNum()
        {
            return cc_num;
        }

        public string getPurchaseCCExpDate()
        {
            return cc_exp_date;
        }
 
        public string getPurchaseCCCVV()
        {
            return cc_cvv;
        }

        public string getPurchaseCCZipcode()
        {
            return cc_zip;
        }

        public string getPurchaseChargeID()
        {
            return charge_id;
        }

        public string getPurchasePaymentType()
        {
            return payment_type;
        }

        public string getPurchaseSubtotal()
        {
            return subtotal;
        }

        public string getPurchaseServiceFee()
        {
            return service_fee;
        }

        public string getPurchaseDeliveryFee()
        {
            return delivery_fee;
        }

        public string getPurchaseDriveTip()
        {
            return driver_tip;
        }

        public string getPurchaseTaxes()
        {
            return taxes;
        }

        public void setPurchaseCustomerUID(string pur_customer_uid)
        {
            this.pur_customer_uid = pur_customer_uid;
        }

        public void setPurchaseBusinessUID(string pur_business_uid)
        {
            this.pur_business_uid = pur_business_uid;
        }

        public void setPurchaseItems(ObservableCollection<PurchasedItem> items)
        {
            this.items = items;
        }

        public void setPurchaseInstructions(string order_instructions)
        {
            this.order_instructions = order_instructions;
        }

        public void setPurchaseDeliveryInstructions(string delivery_instructions)
        {
            this.delivery_instructions = delivery_instructions;
        }

        public void setPurchaseOrderType(string order_type)
        {
            this.order_type = order_type;
        }

        public void setPurchaseFirstName(string delivery_first_name)
        {
            this.delivery_first_name = delivery_first_name;
        }

        public void setPurchaseLastName(string delivery_last_name)
        {
            this.delivery_last_name = delivery_last_name;
        }

        public void setPurchasePhoneNumber(string delivery_phone_num)
        {
            this.delivery_phone_num = delivery_phone_num;
        }

        public void setPurchaseEmail(string delivery_email)
        {
            this.delivery_email = delivery_email;
        }

        public void setPurchaseAddress(string delivery_address)
        {
            this.delivery_address = delivery_address;
        }

        public void setPurchaseUnit(string delivery_unit)
        {
            this.delivery_unit = delivery_unit;
        }

        public void setPurchaseCity(string delivery_city)
        {
            this.delivery_city = delivery_city;
        }

        public void setPurchaseState(string delivery_state)
        {
            this.delivery_state = delivery_state;
        }

        public void setPurchaseZipcode(string delivery_zip)
        {
            this.delivery_zip = delivery_zip;
        }

        public void setPurchaseLatitude(string delivery_latitude)
        {
            this.delivery_latitude = delivery_latitude;
        }

        public void setPurchaseLongitude(string delivery_longitude)
        {
            this.delivery_longitude = delivery_longitude;
        }

        public void setPurchaseNotes(string purchase_notes)
        {
            this.purchase_notes = purchase_notes;
        }

        public void setPurchaseDeliveryDate(string start_delivery_date)
        {
            this.start_delivery_date = start_delivery_date;
        }

        public void setPurchaseCoupoID(string pay_coupon_id)
        {
            this.pay_coupon_id = pay_coupon_id;
        }

        public void setPurchaseAmountDue(string amount_due)
        {
            this.amount_due = amount_due;
        }

        public void setPurchaseDiscount(string amount_discount)
        {
            this.amount_discount = amount_discount;
        }

        public void setPurchasePaid(string amount_paid)
        {
            this.amount_paid = amount_paid;
        }

        public void setPurchaseAddon(string info_is_Addon)
        {
            this.info_is_Addon = info_is_Addon;
        }

        public void setPurchaseCCNum(string cc_num)
        {
            this.cc_num = cc_num;
        }

        public void setPurchaseCCExpDate(string cc_exp_date)
        {
            this.cc_exp_date = cc_exp_date;
        }

        public void setPurchaseCCCVV(string cc_cvv)
        {
            this.cc_cvv = cc_cvv;
        }

        public void setPurchaseCCZipcode(string cc_zip)
        {
            this.cc_zip = cc_zip;
        }

        public void setPurchaseChargeID(string charge_id)
        {
            this.charge_id = charge_id;
        }

        public void setPurchasePaymentType(string payment_type)
        {
            this.payment_type = payment_type;
        }

        public void setPurchaseSubtotal(string subtotal)
        {
            this.subtotal = subtotal;
        }

        public void setPurchaseServiceFee(string service_fee)
        {
            this.service_fee = service_fee;
        }

        public void setPurchaseDeliveryFee(string delivery_fee)
        {
            this.delivery_fee = delivery_fee;
        }

        public void setPurchaseDriveTip(string driver_tip)
        {
            this.driver_tip = driver_tip;
        }

        public void setPurchaseTaxes(string taxes)
        {
            this.taxes = taxes;
        }
    }
}
