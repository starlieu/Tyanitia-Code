public interface IHealthModifier : IModifier
{
    double ApplyHealthResult(bool hit, double health);
}
