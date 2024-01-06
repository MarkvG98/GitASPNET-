using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GitProjektWHS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionsController : ControllerBase
    {
        private readonly VersionsContext _db;

        public VersionsController(VersionsContext context)
        {
            _db = context;
        }
       
        [HttpPost] //create
        public IActionResult CreateVersionsObjekt(VersionsDatei piObjekt)
        {
            _db.Datei.Add(piObjekt);
            _db.SaveChanges();

            return CreatedAtAction("GetObjekt", new { id = piObjekt.Id}, piObjekt);
        }

        [HttpGet]
        public IActionResult GetVersionsObjekt(int piId)
        {
            var versionsDateiFromDb = _db.Datei.SingleOrDefault(b => b.Id == piId);

            if (versionsDateiFromDb == null)
            {
                return NotFound();
            }
            return Ok(versionsDateiFromDb);
        }

        [HttpPut]
        public IActionResult UpdateVersionObjekt(VersionsDatei piObjekt)
        {
            var versionsDateiFromDb = _db.Datei.SingleOrDefault(b => b.Id == piObjekt.Id);
            if (versionsDateiFromDb == null)
            {
                return NotFound();
            }
            versionsDateiFromDb.Lock = piObjekt.Lock;

            _db.SaveChanges();

            return Ok(piObjekt.Lock ? "Objekt geperrt" : "Objekt entsperrt");
        }

        [HttpDelete]
        public IActionResult DeleteVersionsObjekt(VersionsDatei piObjekt)
        {
            var versionsDateiFromDb = _db.Datei.SingleOrDefault(b => b.Id == piObjekt.Id);
            if (versionsDateiFromDb == null)
            {
                return NotFound();
            }

            _db.Remove(piObjekt);
            _db.SaveChanges();

            return Ok();
        }

    }
}
