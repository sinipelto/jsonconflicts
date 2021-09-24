namespace ConflictManager.Backend.Models
{
    public enum Status
    {
        OK = 200,
        NOTFOUND = 404,
        CONFLICT = 409,
        UNHANDLED = 500,
    }
}