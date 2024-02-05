using GitProjektWHS.Models;
using Commons.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/Versions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VersionObject>>> GetVersions()
        {
            return await _db.Versionen.ToListAsync();
        }

        // GET: api/Versions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VersionObject>> GetVersion(long id)
        {
            var version = await _db.Versionen.FindAsync(id);

            if (version == null)
            {
                return NotFound();
            }

            return version;
        }

        // PUT: api/Versions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVersion(long id, VersionObject version)
        {
            if (id != version.Id)
            {
                return BadRequest();
            }

            _db.Entry(version).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VersionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Versions
        [HttpPost]
        public async Task<ActionResult<VersionObject>> PostVersion(VersionObject version)
        {
            _db.Versionen.Add(version);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVersion), new { id = version.Id }, version);
        }

        // DELETE: api/Versions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVersion(long id)
        {
            var version = await _db.Versionen.FindAsync(id);
            if (version == null)
            {
                return NotFound();
            }

            _db.Versionen.Remove(version);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool VersionExists(long id)
        {
            return _db.Versionen.Any(e => e.Id == id);
        }
    }
}
