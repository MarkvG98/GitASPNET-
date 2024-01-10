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
        public IActionResult CreateVersionsDatei(VersionsDatei piDatei)
        {
            _db.Datei.Add(piDatei);
            _db.SaveChanges();

            return CreatedAtAction("GetDatei", new { id = piDatei.Id}, piDatei);
        }
        [HttpPost]
        public IActionResult CreateVersionsObjekt(VersionsObjekt piVersionObjekt)
        {
            _db.Versions.Add(piVersionObjekt);
            _db.SaveChanges();

            return CreatedAtAction("GetObject",new { id = piVersionObjekt.ID, piVersionObjekt});
        }

        [HttpGet]
        public IActionResult GetVersionsDatei(int piId)
        {
            var versionsDateiFromDb = _db.Datei.SingleOrDefault(b => b.Id == piId);

            if (versionsDateiFromDb == null)
            {
                return NotFound();
            }
            return Ok(versionsDateiFromDb);
        }
        [HttpGet]
        public IActionResult GetVersionsObjekt(int piId)
        {
            var versionsObjektFromDb = _db.Versions.SingleOrDefault(b => b.ID == piId);

            if (versionsObjektFromDb == null)
            {
                return NotFound();
            }
            return Ok(versionsObjektFromDb);
        }

        [HttpPut]
        public IActionResult UpdateVersionsDatei(VersionsDatei piObjekt)
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
        [HttpPut]
        public IActionResult UpdateVersionsObjekt(VersionsObjekt piObjekt)
        {
            var versionsObjektFromDb = _db.Versions.SingleOrDefault(b => b.ID == piObjekt.ID);
            if (versionsObjektFromDb == null)
            {
                return NotFound();
            }
            versionsObjektFromDb = piObjekt;

            _db.SaveChanges();

            return Ok("Objekt" + ": " + piObjekt.ID.ToString() + " " +"wurde aktualisiert");
        }
        [HttpDelete]
        public IActionResult DeleteVersionsDatei(VersionsDatei piObjekt)
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
