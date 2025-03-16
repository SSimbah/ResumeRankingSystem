using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Domain.DataAccess;
using Domain.Entities;

namespace ResumeRankingSystem.Controllers
{
    public class JobPostingsController : AuthenticatedController
    {
        private readonly DatabaseDbContext _context;

        public JobPostingsController(DatabaseDbContext context)
        {
            _context = context;
        }

        // GET: JobPostings
        public async Task<IActionResult> Index()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToLogin();
            }
            var databaseDbContext = _context.JobPostings.Include(j => j.User);
            return View(await databaseDbContext.ToListAsync());
        }



        // GET: JobPostings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToLogin();
            }
            if (id == null)
            {
                return NotFound();
            }

            var jobPosting = await _context.JobPostings
                .Include(j => j.User)
                .FirstOrDefaultAsync(m => m.JobId == id);
            if (jobPosting == null)
            {
                return NotFound();
            }

            // Retrieve UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToLogin();
            }

            // Pass the logged-in user's UserId to the view
            ViewBag.LoggedInUserId = userId;

            return View(jobPosting);
        }
        //// GET: JobPostings/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    // Check if the user is logged in
        //    if (!IsUserLoggedIn())
        //    {
        //        return RedirectToLogin();
        //    }

        //    // Validate that the ID is provided
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    // Retrieve the job posting from the database
        //    //var jobPosting = await _context.JobPostings.FindAsync(id);
        //    // Retrieve the job posting along with related requirements from the database
        //    var jobPosting = await _context.JobPostings
        //        .Include(j => j.User)
        //        .Include(j => j.SkillRequirements)
        //        .Include(j => j.EducationRequirements)
        //        .Include(j => j.ExperienceRequirements)
        //        .FirstOrDefaultAsync(m => m.JobId == id);

        //    if (jobPosting == null)
        //    {
        //        return NotFound();
        //    }

        //    // Retrieve UserId from session
        //    var userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId == null)
        //    {
        //        return RedirectToLogin();
        //    }

        //    // Pass the logged-in user's UserId to the view
        //    ViewBag.LoggedInUserId = userId;

        //    return View(jobPosting);
        //}

        // GET: JobPostings/Create
        public IActionResult Create()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToLogin();
            }
            //ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email");
            // Retrieve UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToLogin();
            }

            // Fetch user details from database
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
            {
                return RedirectToLogin();
            }

            ViewBag.UserEmail = user.Email; // Pass email to view
            ViewBag.UserId = user.UserId;   // Pass UserId to view
            return View();
        }

        // POST: JobPostings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Title,Description,CreatedAt")] JobPosting jobPosting)
        //{
        //    //if (ModelState.IsValid)
        //    //{
        //    //    _context.Add(jobPosting);
        //    //    await _context.SaveChangesAsync();
        //    //    return RedirectToAction(nameof(Index));
        //    //}
        //    //ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", jobPosting.UserId);
        //    //return View(jobPosting);
        //    if (!IsUserLoggedIn())
        //    {
        //        return RedirectToLogin();
        //    }

        //    // Retrieve UserId from session
        //    var userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId == null)
        //    {
        //        return RedirectToLogin();
        //    }

        //    jobPosting.UserId = userId.Value; // Assign UserId automatically

        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(jobPosting);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    // Retrieve user email again if the form needs to be reloaded
        //    var user = _context.Users.FirstOrDefault(u => u.UserId == jobPosting.UserId);
        //    ViewBag.UserEmail = user?.Email;

        //    // Redirect to the SkillRequirements view
        //    return RedirectToAction("SkillRequirements");

        //    //return View(jobPosting);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,CreatedAt")] JobPosting jobPosting)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToLogin();
            }

            // Retrieve UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToLogin();
            }

            // Assign UserId automatically
            jobPosting.UserId = userId.Value;

            if (ModelState.IsValid)
            {
                // Add the new job posting to the database
                _context.Add(jobPosting);
                await _context.SaveChangesAsync();

                // After saving, the jobPosting object will have its JobId populated by EF Core
                int newJobId = jobPosting.JobId;

                // Redirect to the SkillRequirements view and pass the JobId
                return RedirectToAction("Create", "SkillRequirements", new { jobId = newJobId });
            }

            // If we got this far, something failed; redisplay form
            var user = _context.Users.FirstOrDefault(u => u.UserId == jobPosting.UserId);
            ViewBag.UserEmail = user?.Email;

            return View(jobPosting);
        }

        // GET: JobPostings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToLogin();
            }
            if (id == null)
            {
                return NotFound();
            }

            var jobPosting = await _context.JobPostings.FindAsync(id);
            if (jobPosting == null)
            {
                return NotFound();
            }

            // Retrieve UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToLogin();
            }

            // Fetch user details
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
            {
                return RedirectToLogin();
            }

            ViewBag.UserEmail = user.Email; // Pass email to view
            ViewBag.UserId = user.UserId;   // Pass UserId to view
            return View(jobPosting);
        }

        // POST: JobPostings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("JobId,UserId,Title,Description,CreatedAt")] JobPosting jobPosting)
        {
            if (id != jobPosting.JobId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobPosting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobPostingExists(jobPosting.JobId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", jobPosting.UserId);
            return View(jobPosting);
        }

        // GET: JobPostings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToLogin();
            }
            if (id == null)
            {
                return NotFound();
            }

            var jobPosting = await _context.JobPostings
                .Include(j => j.User)
                .FirstOrDefaultAsync(m => m.JobId == id);
            if (jobPosting == null)
            {
                return NotFound();
            }

            return View(jobPosting);
        }

        // POST: JobPostings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jobPosting = await _context.JobPostings.FindAsync(id);
            if (jobPosting != null)
            {
                _context.JobPostings.Remove(jobPosting);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobPostingExists(int id)
        {
            return _context.JobPostings.Any(e => e.JobId == id);
        }
    }
}
