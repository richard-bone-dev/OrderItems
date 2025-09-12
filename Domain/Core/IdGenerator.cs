namespace Api.Domain.Core;

// Simple ID generator for PoC
public static class IdGenerator
{
    private static int _order = 0;
    private static int _payment = 0;
    private static int _user = 0;
    public static int NextUserId() => ++_user;
    public static int NextOrderId() => ++_order;
    public static int NextPaymentId() => ++_payment;
}
