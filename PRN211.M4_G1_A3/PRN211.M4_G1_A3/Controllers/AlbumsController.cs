using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_WebMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_WebMVC.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly MusicStoreContext _context;
        private readonly IWebHostEnvironment _environment;
        

        public AlbumsController(MusicStoreContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            
        }

        // GET: Albums
        public async Task<IActionResult> Index(int genreId, string currentTitle)
        {
            List<SelectListItem> genres = new SelectList(_context.Genres, "GenreId", "Name", genreId).ToList();
            genres.Insert(0, new SelectListItem { Value = "0", Text = "All" });
            ViewData["GenreIds"] = genres;

            var musicStoreContext = _context.Albums.Include(a => a.Artist).Include(a => a.Genre)
                .Where(a => a.GenreId == (genreId == 0 ? a.GenreId : genreId)
                && a.Title.Contains(currentTitle ?? ""));

            ViewData["CurrentTitle"] = currentTitle;
            return View(await musicStoreContext.ToListAsync());
        }

        // GET: Albums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Genre)
                .FirstOrDefaultAsync(m => m.AlbumId == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Albums/Create
        public IActionResult Create()
        {
            ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name");
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name");
            return View();
        }

        // POST: Albums/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlbumId,GenreId,ArtistId,Title,Price,AlbumUrl")] Album album, IFormFile file)
        {

            string dir = Path.Combine(_environment.WebRootPath, "Images");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string filePath = Path.Combine(dir, file.FileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            album.AlbumUrl = "/Images/" + file.FileName;

            //if (ModelState.IsValid)
            try
            {
                _context.Albums.Add(album);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", album.ArtistId);
                ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", album.GenreId);
                return View(album);
            }
        }

        // GET: Albums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", album.ArtistId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", album.GenreId);
            return View(album);
        }

        // POST: Albums/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlbumId,GenreId,ArtistId,Title,Price,AlbumUrl")] Album album , IFormFile file)
        {

            if (id != album.AlbumId)
            {
                return NotFound();
            }
            //if (!ModelState.IsValid)
            {
                try
                {
                    string dir = Path.Combine(_environment.WebRootPath, "Images");
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    string filePath = Path.Combine(dir, file.FileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    album.AlbumUrl = "/Images/" + file.FileName;
                    _context.Albums.Update(album);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.AlbumId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch(ArgumentNullException ex)
                {
                    throw ex;
                }
                
            }
            ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", album.ArtistId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", album.GenreId);
            return View(album);
        }

        // GET: Albums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Genre)
                .FirstOrDefaultAsync(m => m.AlbumId == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.Albums == null)
                {
                    return Problem("Entity set 'MusicStoreContext.Albums'  is null.");
                }
                var album = await _context.Albums.FindAsync(id);
                if (album != null)
                {
                    _context.Albums.Remove(album);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool AlbumExists(int id)
        {
            return _context.Albums.Any(e => e.AlbumId == id);
        }
    }
}
