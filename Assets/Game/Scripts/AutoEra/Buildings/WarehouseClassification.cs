namespace AutoEra.Buildings
{
    public enum CargoItemClass { Unknown, Gold, CommonResource, Seed, FarmProduct, LocalPhysicalItem, Component, MachineCarrier }
    public enum WarehouseDestination { Rejected, GlobalBalance, LocalInventory, ComponentLibrary, MachineLibrary }
    public static class WarehouseClassification
    {
        /// <summary>Routing only; settlement and identity transfer are performed by the single cargo authority.</summary>
        public static WarehouseDestination Classify(CargoItemClass kind)
        {
            switch (kind)
            {
                case CargoItemClass.Gold: case CargoItemClass.CommonResource: return WarehouseDestination.GlobalBalance;
                case CargoItemClass.Seed: case CargoItemClass.FarmProduct: case CargoItemClass.LocalPhysicalItem: return WarehouseDestination.LocalInventory;
                case CargoItemClass.Component: return WarehouseDestination.ComponentLibrary;
                case CargoItemClass.MachineCarrier: return WarehouseDestination.MachineLibrary;
                default: return WarehouseDestination.Rejected;
            }
        }
        public static bool OccupiesLocalCapacity(CargoItemClass kind) => Classify(kind) == WarehouseDestination.LocalInventory;
    }
}
