using GitProjektWHS.Models;
using Commons.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GitProjektWHS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly VersionsContext _db;

        public FilesController(VersionsContext context)
        {
            _db = context;
        }

        // GET: api/Files
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileObject>>> GetFiles()
        {
            return await _db.Dateien.ToListAsync();
        }

        // GET: api/Files/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FileObject>> GetFile(long id)
        {
            var datei = await _db.Dateien.FindAsync(id);

            if (datei == null)
            {
                return NotFound();
            }

            return datei;
        }

        // PUT: api/Files/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFile(long id, FileObject datei)
        {
            if (id != datei.Id)
            {
                return BadRequest();
            }

            _db.Entry(datei).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FileExists(id))
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

        // POST: api/Files
        [HttpPost]
        public async Task<ActionResult<FileObject>> PostFile(FileObject datei)
        {
            _db.Dateien.Add(datei);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFile), new { id = datei.Id }, datei);
        }

        // DELETE: api/Files/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(long id)
        {
            var datei = await _db.Dateien.FindAsync(id);
            if (datei == null)
            {
                return NotFound();
            }

            _db.Dateien.Remove(datei);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool FileExists(long id)
        {
            return _db.Dateien.Any(e => e.Id == id);
        }
    }
}
