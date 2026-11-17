namespace FastFoodAPI.Exceptions;

public sealed class OrderConflictException : Exception
{
    public OrderConflictException()
        : base(
            "O pedido foi alterado por outro utilizador. " +
            "Atualiza os dados e tenta novamente.")
    {
    }
}