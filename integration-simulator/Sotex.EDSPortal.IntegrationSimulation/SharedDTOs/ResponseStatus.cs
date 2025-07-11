﻿namespace Sotex.EDSPortal.IntegrationSimulation.SharedDTOs
{
    public enum ResponseStatus
    {
        OK = 200,
        Created = 201,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,
        InternalServerError = 500,
        ServiceUnavailable = 503,
        Pending = 600
    }
}
