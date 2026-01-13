using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Models;

namespace SchoolManagement.Controllers
{
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Students?page=1
        public async Task<IActionResult> Index(int page = 1)
        {
            var total = await _context.Students.CountAsync();
            var students = await _context.Students
                .Include(s => s.School)
                .OrderBy(s => s.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);
            return View(students);
        }

        // GET: Students/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Schools = await _context.Schools.ToListAsync();
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,StudentId,Email,Phone,SchoolId")] Student student)
        {
            ViewBag.Schools = await _context.Schools.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(student);
            }

            // Check school exists
            if (!await _context.Schools.AnyAsync(s => s.Id == student.SchoolId))
            {
                ModelState.AddModelError("SchoolId", "Selected school does not exist.");
                return View(student);
            }

            // Check unique StudentId and Email
            if (await _context.Students.AnyAsync(s => s.StudentId == student.StudentId))
            {
                ModelState.AddModelError("StudentId", "Student ID must be unique.");
                return View(student);
            }
            if (await _context.Students.AnyAsync(s => s.Email == student.Email))
            {
                ModelState.AddModelError("Email", "Email must be unique.");
                return View(student);
            }

            try
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the student: " + ex.Message);
                return View(student);
            }
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            ViewBag.Schools = await _context.Schools.ToListAsync();
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,StudentId,Email,Phone,SchoolId")] Student student)
        {
            if (id != student.Id) return BadRequest();

            ViewBag.Schools = await _context.Schools.ToListAsync();

            if (!ModelState.IsValid) return View(student);

            if (!await _context.Schools.AnyAsync(s => s.Id == student.SchoolId))
            {
                ModelState.AddModelError("SchoolId", "Selected school does not exist.");
                return View(student);
            }

            // Unique checks excluding current record
            if (await _context.Students.AnyAsync(s => s.StudentId == student.StudentId && s.Id != student.Id))
            {
                ModelState.AddModelError("StudentId", "Student ID must be unique.");
                return View(student);
            }
            if (await _context.Students.AnyAsync(s => s.Email == student.Email && s.Id != student.Id))
            {
                ModelState.AddModelError("Email", "Email must be unique.");
                return View(student);
            }

            try
            {
                _context.Entry(student).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Students.AnyAsync(e => e.Id == student.Id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the student: " + ex.Message);
                return View(student);
            }
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students.Include(s => s.School).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            try
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the student: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
