namespace FluentCsvMachine.Test.Models
{
    public class OrdersDs
    {
        //Region,Country,Item Type, Sales Channel,Order Priority, Order Date,Order ID, Ship Date,Units Sold, Unit Price,UnitCost, TotalRevenue,TotalCost, TotalProfit
        public OrdersDsRegion Region { get; set; }

        public OrdersDsCountry Country { get; set; }

        public OrdersDsItemType ItemType { get; set; }

        public OrdersDsSalesChannel SalesChannel { get; set; }

        public char OrderPriority { get; set; }

        public DateTime OrderDate { get; set; }

        public long OrderId { get; set; }

        public DateTime ShipDate { get; set; }

        public int UnitsSold { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalProfit { get; set; }
    }

    public enum OrdersDsRegion
    {
        Asia,
        SubSaharanAfrica,
        MiddleEastandNorthAfrica,
        AustraliaandOceania,
        Europe,
        CentralAmericaandtheCaribbean,
        NorthAmerica
    }

    public enum OrdersDsCountry
    {
        Afghanistan,
        Albania,
        Algeria,
        Andorra,
        Angola,
        AntiguaandBarbuda,
        Armenia,
        Australia,
        Austria,
        Azerbaijan,
        Bahrain,
        Bangladesh,
        Barbados,
        Belarus,
        Belgium,
        Belize,
        Benin,
        Bhutan,
        BosniaandHerzegovina,
        Botswana,
        Brunei,
        Bulgaria,
        BurkinaFaso,
        Burundi,
        Cambodia,
        Cameroon,
        Canada,
        CapeVerde,
        CentralAfricanRepublic,
        Chad,
        China,
        Comoros,
        CostaRica,
        CotedIvoire,
        Croatia,
        Cuba,
        Cyprus,
        CzechRepublic,
        DemocraticRepublicoftheCongo,
        Denmark,
        Djibouti,
        Dominica,
        DominicanRepublic,
        EastTimor,
        Egypt,
        ElSalvador,
        EquatorialGuinea,
        Eritrea,
        Estonia,
        Ethiopia,
        FederatedStatesofMicronesia,
        Fiji,
        Finland,
        France,
        Gabon,
        Georgia,
        Germany,
        Ghana,
        Greece,
        Greenland,
        Grenada,
        Guatemala,
        Guinea,
        GuineaBissau,
        Haiti,
        Honduras,
        Hungary,
        Iceland,
        India,
        Indonesia,
        Iran,
        Iraq,
        Ireland,
        Israel,
        Italy,
        Jamaica,
        Japan,
        Jordan,
        Kazakhstan,
        Kenya,
        Kiribati,
        Kosovo,
        Kuwait,
        Kyrgyzstan,
        Laos,
        Latvia,
        Lebanon,
        Lesotho,
        Liberia,
        Libya,
        Liechtenstein,
        Lithuania,
        Luxembourg,
        Macedonia,
        Madagascar,
        Malawi,
        Malaysia,
        Maldives,
        Mali,
        Malta,
        MarshallIslands,
        Mauritania,
        Mauritius,
        Mexico,
        Moldova,
        Monaco,
        Mongolia,
        Montenegro,
        Morocco,
        Mozambique,
        Myanmar,
        Namibia,
        Nauru,
        Nepal,
        Netherlands,
        NewZealand,
        Nicaragua,
        Niger,
        Nigeria,
        NorthKorea,
        Norway,
        Oman,
        Pakistan,
        Palau,
        Panama,
        PapuaNewGuinea,
        Philippines,
        Poland,
        Portugal,
        Qatar,
        RepublicoftheCongo,
        Romania,
        Russia,
        Rwanda,
        SaintKittsandNevis,
        SaintLucia,
        SaintVincentandtheGrenadines,
        Samoa,
        SanMarino,
        SaoTomeandPrincipe,
        SaudiArabia,
        Senegal,
        Serbia,
        Seychelles,
        SierraLeone,
        Singapore,
        Slovakia,
        Slovenia,
        SolomonIslands,
        Somalia,
        SouthAfrica,
        SouthKorea,
        SouthSudan,
        Spain,
        SriLanka,
        Sudan,
        Swaziland,
        Sweden,
        Switzerland,
        Syria,
        Taiwan,
        Tajikistan,
        Tanzania,
        Thailand,
        TheBahamas,
        TheGambia,
        Togo,
        Tonga,
        TrinidadandTobago,
        Tunisia,
        Turkey,
        Turkmenistan,
        Tuvalu,
        Uganda,
        Ukraine,
        UnitedArabEmirates,
        UnitedKingdom,
        UnitedStatesofAmerica,
        Uzbekistan,
        Vanuatu,
        VaticanCity,
        Vietnam,
        Yemen,
        Zambia,
        Zimbabwe,
    }

    public enum OrdersDsItemType
    {
        Snacks,
        Meat,
        Fruits,
        Vegetables,
        OfficeSupplies,
        BabyFood,
        Beverages,
        Cereal,
        Household,
        Cosmetics,
        Clothes,
        PersonalCare
    }

    public enum OrdersDsSalesChannel
    {
        Online,
        Offline
    }
}
