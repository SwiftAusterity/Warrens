namespace NetMud.Models.Admin.PartialInterfaces
{

    public interface IDimensionalEntityViewModel
    {
        int DimensionalModelLength { get; set; }
        int DimensionalModelHeight { get; set; }
        int DimensionalModelWidth { get; set; }
        int DimensionalModelVacuity { get; set; }
        int DimensionalModelCavitation { get; set; }
    }
}
