namespace ConflictManager.Backend.Models
{
    internal enum Status
    {
        OK = 200,
        INVALIDREQUEST = 400,
        NOTFOUND = 404,
        CONFLICT = 409,
        UNHANDLED = 500,
    }
}