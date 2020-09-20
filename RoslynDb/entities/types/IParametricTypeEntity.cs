namespace J4JSoftware.Roslyn
{
    public interface IParametricTypeEntity
    {
        ParametricTypeConstraint Constraints { get; set; }
        int? ContainerID { get; set; }
        object? Container { get; set; }
    }
}