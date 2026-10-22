namespace FactoryDesignPattern;

/// <summary>The plain data both catalogs return. The client sees this, never a backend row.</summary>
public sealed record Product(string Sku, string Name, decimal Price);
