namespace ShareIT.Constant
{
    public static class Permissions
    {
        public static List<string> GeneratePermissionsList(string module)
        {
            return new List<string>()
            {
                $"Permissions.{module}.View",
                $"Permissions.{module}.Create",
                $"Permissions.{module}.Edit",
                $"Permissions.{module}.Delete"
            };
        }

        public static List<string> GenerateAllPermissions()
        {
            var allPermissions = new List<string>();

            var modules = Enum.GetValues(typeof(Modules));

            foreach (var module in modules)
                allPermissions.AddRange(GeneratePermissionsList(module.ToString()));

            return allPermissions;
        }
        public static class Asset
        {
            public const string View = "Permissions.Asset.View";
            public const string Create = "Permissions.Asset.Create";
            public const string Edit = "Permissions.Asset.Edit";
            public const string Delete = "Permissions.Asset.Delete";
        }
        public static class Alert
        {
            public const string View = "Permissions.Alert.View";
            public const string Create = "Permissions.Alert.Create";
            public const string Edit = "Permissions.Alert.Edit";
            public const string Delete = "Permissions.Alert.Delete";
        }
        public static class Brand
        {
            public const string View = "Permissions.Brand.View";
            public const string Create = "Permissions.Brand.Create";
            public const string Edit = "Permissions.Brand.Edit";
            public const string Delete = "Permissions.Brand.Delete";
        }
        public static class BrandModels
        {
            public const string View = "Permissions.BrandModels.View";
            public const string Create = "Permissions.BrandModels.Create";
            public const string Edit = "Permissions.BrandModels.Edit";
            public const string Delete = "Permissions.BrandModels.Delete";
        }
        public static class Color
        {
            public const string View = "Permissions.Color.View";
            public const string Create = "Permissions.Color.Create";
            public const string Edit = "Permissions.Color.Edit";
            public const string Delete = "Permissions.Color.Delete";
        }
        public static class CM
        {
            public const string View = "Permissions.CM.View";
            public const string Create = "Permissions.CM.Create";
            public const string Edit = "Permissions.CM.Edit";
            public const string Delete = "Permissions.CM.Delete";
        }
        public static class CostCenter
        {
            public const string View = "Permissions.CostCenter.View";
            public const string Create = "Permissions.CostCenter.Create";
            public const string Edit = "Permissions.CostCenter.Edit";
            public const string Delete = "Permissions.CostCenter.Delete";
        }
        public static class Daily
        {
            public const string View = "Permissions.Daily.View";
            public const string Create = "Permissions.Daily.Create";
            public const string Edit = "Permissions.Daily.Edit";
            public const string Delete = "Permissions.Daily.Delete";
        }
        public static class ManPower
        {
            public const string View = "Permissions.ManPower.View";
            public const string Create = "Permissions.ManPower.Create";
            public const string Edit = "Permissions.ManPower.Edit";
            public const string Delete = "Permissions.ManPower.Delete";
        }
        public static class MaintancePlan
        {
            public const string View = "Permissions.MaintancePlan.View";
            public const string Create = "Permissions.MaintancePlan.Create";
            public const string Edit = "Permissions.MaintancePlan.Edit";
            public const string Delete = "Permissions.MaintancePlan.Delete";
        }
        public static class Owner
        {
            public const string View = "Permissions.Owner.View";
            public const string Create = "Permissions.Owner.Create";
            public const string Edit = "Permissions.Owner.Edit";
            public const string Delete = "Permissions.Owner.Delete";
        }
        public static class Zone
        {
            public const string View = "Permissions.Zone.View";
            public const string Create = "Permissions.Zone.Create";
            public const string Edit = "Permissions.Zone.Edit";
            public const string Delete = "Permissions.Zone.Delete";
        }
        public static class PTasks
        {
            public const string View = "Permissions.PTasks.View";
            public const string Create = "Permissions.PTasks.Create";
            public const string Edit = "Permissions.PTasks.Edit";
            public const string Delete = "Permissions.PTasks.Delete";
        }
        public static class SparePart
        {
            public const string View = "Permissions.SparePart.View";
            public const string Create = "Permissions.SparePart.Create";
            public const string Edit = "Permissions.SparePart.Edit";
            public const string Delete = "Permissions.SparePart.Delete";
        }
        public static class Types
        {
            public const string View = "Permissions.Types.View";
            public const string Create = "Permissions.Types.Create";
            public const string Edit = "Permissions.Types.Edit";
            public const string Delete = "Permissions.Types.Delete";
        }
        public static class WorkOrder
        {
            public const string View = "Permissions.WorkOrder.View";
            public const string Create = "Permissions.WorkOrder.Create";
            public const string Edit = "Permissions.WorkOrder.Edit";
            public const string Delete = "Permissions.WorkOrder.Delete";
        }
        public static class Sector

        {
            public const string View = "Permissions.Sector.View";
            public const string Create = "Permissions.Sector.Create";
            public const string Edit = "Permissions.Sector.Edit";
            public const string Delete = "Permissions.Sector.Delete";
        }
        public static class Pricing

        {
            public const string View = "Permissions.Pricing.View";
            public const string Create = "Permissions.Pricing.Create";
            public const string Edit = "Permissions.Pricing.Edit";
            public const string Delete = "Permissions.Pricing.Delete";
        }
        public static class WorkShop

        {
            public const string View = "Permissions.WorkShop.View";
            public const string Create = "Permissions.WorkShop.Create";
            public const string Edit = "Permissions.WorkShop.Edit";
            public const string Delete = "Permissions.WorkShop.Delete";
        }


    }

}
