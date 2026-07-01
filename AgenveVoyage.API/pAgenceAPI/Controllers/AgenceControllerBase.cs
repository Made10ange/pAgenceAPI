using Microsoft.AspNetCore.Mvc;

namespace pAgenceAPI.Controllers;

public abstract class AgenceControllerBase : ControllerBase
{
    protected int? AgenceId
    {
        get
        {
            var val = Request.Headers["X-Agence-Id"].FirstOrDefault();
            return int.TryParse(val, out var id) ? id : null;
        }
    }

    protected int? UserId
    {
        get
        {
            var val = Request.Headers["X-User-Id"].FirstOrDefault();
            return int.TryParse(val, out var id) ? id : null;
        }
    }
}
