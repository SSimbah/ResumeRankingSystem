using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Domain.DataAccess;
using Domain.Entities;
using System.Text.Json;

namespace ResumeRankingSystem.Controllers
{
    public class ApplicantsController : Controller
    {
        private readonly DatabaseDbContext _context;

        public ApplicantsController(DatabaseDbContext context)
        {
            _context = context;
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (applicant == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }

            // Redirect to the index page or dashboard after successful login
            // return RedirectToAction(nameof(Index));
            // Store user information in session
            HttpContext.Session.SetInt32("ApplicantId", applicant.ApplicantId);
            HttpContext.Session.SetString("Username", applicant.Username);

            return RedirectToAction(nameof(Index), "Home");
        }

        // GET: Users/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Applicants
        public async Task<IActionResult> Index()
        {
            return View(await _context.Applicants.ToListAsync());
        }

        // GET: Applicants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(m => m.ApplicantId == id);
            if (applicant == null)
            {
                return NotFound();
            }

            return View(applicant);
        }

        // GET: Applicants/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}
        [HttpGet]
        public IActionResult Create(string parsedFields)
        {
            // Deserialize the JSON data back into a dictionary
            var parsedFieldsDict = JsonSerializer.Deserialize<Dictionary<string, string>>(parsedFields);

            var applicant = new Applicant
            {
                FirstName = parsedFieldsDict?.GetValueOrDefault("fname_input"),
                MiddleName = parsedFieldsDict?.GetValueOrDefault("mname_input"),
                LastName = parsedFieldsDict?.GetValueOrDefault("lname_input"),
                Objective = parsedFieldsDict?.GetValueOrDefault("objective_input"),
                Email = parsedFieldsDict?.GetValueOrDefault("email_input"),
                PhoneNumber = parsedFieldsDict?.GetValueOrDefault("phone_input"),
                Address = parsedFieldsDict?.GetValueOrDefault("address_input"),
                Gender = parsedFieldsDict?.GetValueOrDefault("gender_input"),
                Age = int.TryParse(parsedFieldsDict?.GetValueOrDefault("age_input"), out var age) ? age : 0,
                Experience = parsedFieldsDict?.GetValueOrDefault("experience_input"),
                Education = parsedFieldsDict?.GetValueOrDefault("education_input"),
                Skills = parsedFieldsDict?.GetValueOrDefault("skills_input")
            };

            // Pass the applicant model to the next view or action
            return View(applicant);
        }



        // POST: Applicants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Username,Password")] Applicant applicant)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(applicant);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(applicant);
        //}
        // POST: Applicants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Password,FirstName,MiddleName,LastName,Objective,Email,PhoneNumber,Address,Gender,Age,Experience,Education,Skills")] Applicant applicant)
        {
            if (ModelState.IsValid)
            {
                // Save the new applicant to the database
                _context.Add(applicant);
                await _context.SaveChangesAsync();

                // Log the applicant in by setting session values
                HttpContext.Session.SetInt32("ApplicantId", applicant.ApplicantId);
                HttpContext.Session.SetString("Username", applicant.Username);

                // Redirect to a page after login, such as the applicant's details page
                //return RedirectToAction(nameof(Details), new { id = applicant.ApplicantId });

                // Redirect to the UploadProof view
                return RedirectToAction("UploadProof");
            }

            // If model state is not valid, return the view with validation errors
            return View(applicant);
        }

        public IActionResult UploadProof()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProof(List<IFormFile> files)
        {
            if (files == null || !files.Any())
            {
                ModelState.AddModelError(string.Empty, "No files were selected.");
                return View();
            }

            // Retrieve the ApplicantId from session
            var applicantId = HttpContext.Session.GetInt32("ApplicantId");
            if (applicantId == null)
            {
                // Handle the case where the session is missing
                return RedirectToAction("Login", "Applicants");
            }

            var uploadedDocuments = new List<ProofDocument>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Generate a unique file name in the format applicantId_uniqueGenerated.extension
                    var uniqueFileName = $"{applicantId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine("wwwroot/uploads", uniqueFileName);

                    // Save the file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Create a new ProofDocument instance
                    var proofDocument = new ProofDocument
                    {
                        ApplicantId = applicantId.Value,
                        DocumentType = Path.GetExtension(file.FileName).TrimStart('.'), // File type (e.g., pdf)
                        FilePath = $"/uploads/{uniqueFileName}", // Relative path for the file
                    };

                    uploadedDocuments.Add(proofDocument);
                }
            }

            // Save the document records to the database
            if (uploadedDocuments.Any())
            {
                _context.ProofDocuments.AddRange(uploadedDocuments);
                await _context.SaveChangesAsync();
            }

            // Redirect to the Applicant Details page
            return RedirectToAction("Details", "Applicants", new { id = applicantId });
        }




        // GET: Applicants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant == null)
            {
                return NotFound();
            }
            return View(applicant);
        }

        // POST: Applicants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Username,Password")] Applicant applicant)
        {
            if (id != applicant.ApplicantId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applicant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicantExists(applicant.ApplicantId))
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
            return View(applicant);
        }

        // GET: Applicants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(m => m.ApplicantId == id);
            if (applicant == null)
            {
                return NotFound();
            }

            return View(applicant);
        }

        // POST: Applicants/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var applicant = await _context.Applicants.FindAsync(id);
        //    if (applicant != null)
        //    {
        //        _context.Applicants.Remove(applicant);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the applicant to delete
            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant == null)
            {
                return NotFound();
            }

            // Retrieve all associated ProofDocuments
            var proofDocuments = await _context.ProofDocuments
                .Where(d => d.ApplicantId == id)
                .ToListAsync();

            // Delete each file from the directory
            foreach (var document in proofDocuments)
            {
                var filePath = Path.Combine("wwwroot", document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Remove the ProofDocuments from the database
            _context.ProofDocuments.RemoveRange(proofDocuments);

            // Remove the Applicant from the database
            _context.Applicants.Remove(applicant);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect to the Applicants index page
            return RedirectToAction(nameof(Index));
        }


        private bool ApplicantExists(int id)
        {
            return _context.Applicants.Any(e => e.ApplicantId == id);
        }
    }
}
