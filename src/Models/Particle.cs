namespace BTGBrownianMotion.Models;

public  class Particle(float x, float y)
{
    static readonly Random _random = new();

    public float X { get; set; } = x;
    public float Y { get; set; } = y;

    public void Move(float maxStep = 2f)
    {
        X += (float)(_random.NextDouble() * 2 - 1) * maxStep;
        Y += (float)(_random.NextDouble() * 2 - 1) * maxStep;
    }
}
