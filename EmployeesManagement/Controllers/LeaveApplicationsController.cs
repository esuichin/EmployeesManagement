﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeesManagement.Data;
using EmployeesManagement.Models;

namespace EmployeesManagement.Controllers
{
    public class LeaveApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LeaveApplications
        public async Task<IActionResult> Index()
        {
            var awaitingStatus = _context.SystemCodeDetails
                .Include(x => x.systemCode)
                .Where(y => y.systemCode.Code == "LeaveApprovalStatus" &&
                            y.Code == "AwaitingApproval").FirstOrDefault();

            var applicationDbContext = _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .Where(l => l.StatusId == awaitingStatus!.Id);

            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> ApprovedLeaveApplications()
        {
            var approvedStatus = _context.SystemCodeDetails
                .Include(x => x.systemCode)
                .Where(y => y.systemCode.Code == "LeaveApprovalStatus" &&
                            y.Code == "Approved").FirstOrDefault();

            var applicationDbContext = _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .Where(l => l.StatusId == approvedStatus!.Id);

            return View(await applicationDbContext.ToListAsync());
        }


        public async Task<IActionResult> RejectedLeaveApplications()
        {
            var rejectedStatus = _context.SystemCodeDetails
                .Include(x => x.systemCode)
                .Where(y => y.systemCode.Code == "LeaveApprovalStatus" &&
                            y.Code == "Rejected").FirstOrDefault();

            var applicationDbContext = _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .Where(l => l.StatusId == rejectedStatus!.Id);

            return View(await applicationDbContext.ToListAsync());
        }


        // GET: LeaveApplications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (leaveApplication == null)
            {
                return NotFound();
            }

            return View(leaveApplication);
        }
               
        // GET: LeaveApplications/Create
        public IActionResult Create()
        {
            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails
                .Include(x => x.systemCode)
                .Where(y => y.systemCode.Code == "LeaveDuration"), "Id", "Description");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");
            ViewData["StatusId"] = new SelectList(_context.SystemCodeDetails, "Id", "Description");
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leaveApplication == null)
            {
                return NotFound();
            }

            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.systemCode)
                .Where(y => y.systemCode.Code == "LeaveDuration"), "Id", "Description");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");

            return View(leaveApplication);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveLeave(LeaveApplication leave)
        {
            var approvedStatus = _context.SystemCodeDetails
                .Include(x => x.systemCode)
                .Where(y => y.systemCode.Code == "LeaveApprovalStatus" &&
                            y.Code == "Approved").FirstOrDefault();

            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .FirstOrDefaultAsync(m => m.Id == leave.Id);
            if (leaveApplication == null)
            {
                return NotFound();
            }

            leaveApplication.ApprovedOn = DateTime.Now;
            leaveApplication.ApprovedById = "Esther Sui";
            leaveApplication.StatusId = approvedStatus!.Id;
            leaveApplication.ApprovalNotes = leave.ApprovalNotes;

            _context.Update(leaveApplication);
            await _context.SaveChangesAsync();

            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.systemCode).Where(y => y.systemCode.Code == "LeaveDuration"), "Id", "Description");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");

            return RedirectToAction(nameof(Index));
        }

        // POST: LeaveApplications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveApplication leaveApplication)
        {
            var pendingStatus = _context.SystemCodeDetails
                .Include(x => x.systemCode)
                .Where(y => y.Code == "AwaitingApproval" &&
                       y.systemCode.Code == "LeaveApprovalStatus").FirstOrDefaultAsync();

            //if (ModelState.IsValid)
            //{
            //    leaveApplication.CreatedOn = DateTime.Now;
            //    leaveApplication.CreatedById = "Esther Sui";
            //    leaveApplication.StatusId = pendingStatus.Id;
            //    _context.Add(leaveApplication);
            //    await _context.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}

            leaveApplication.CreatedOn = DateTime.Now;
            leaveApplication.CreatedById = "Esther Sui";
            leaveApplication.StatusId = pendingStatus.Id;
            _context.Add(leaveApplication);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));


            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.systemCode)
                .Where(y => y.systemCode.Code == "LeaveDuration"), "Id", "Description", leaveApplication.DurationId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leaveApplication.EmployeeId);
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveApplication.LeaveTypeId);
            //ViewData["StatusId"] = new SelectList(_context.SystemCodeDetails, "Id", "Description", leaveApplication.StatusId);
            
            return View(leaveApplication);
        }

        // GET: LeaveApplications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var pendingStatus = _context.SystemCodeDetails
                .Include(x => x.systemCode)
                .Where(y => y.Code == "Pending"
                    && y.systemCode.Code == "LeaveApprovalStatus").FirstOrDefaultAsync();

            var leaveApplication = await _context.LeaveApplications.FindAsync(id);
            if (leaveApplication == null)
            {
                return NotFound();
            }
            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.systemCode)
                .Where(y => y.systemCode.Code == "LeaveDuration"), "Id", "Description", leaveApplication.DurationId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leaveApplication.EmployeeId);
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveApplication.LeaveTypeId);
            //ViewData["StatusId"] = new SelectList(_context.SystemCodeDetails, "Id", "Description", leaveApplication.StatusId);
            return View(leaveApplication);
        }

        // POST: LeaveApplications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LeaveApplication leaveApplication)
        {
            if (id != leaveApplication.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var pendingStatus = _context.SystemCodeDetails
                .Include(x => x.systemCode)
                .Where(y => y.Code == "Pending"
                    && y.systemCode.Code == "LeaveApprovalStatus").FirstOrDefaultAsync();

                try
                {
                    leaveApplication.ModifiedOn = DateTime.Now;
                    leaveApplication.ModifiedById = "Esther Sui";
                    leaveApplication.StatusId = pendingStatus.Id;
                    _context.Update(leaveApplication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveApplicationExists(leaveApplication.Id))
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
            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails, "Id", "Description", leaveApplication.DurationId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leaveApplication.EmployeeId);
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveApplication.LeaveTypeId);
            //ViewData["StatusId"] = new SelectList(_context.SystemCodeDetails, "Id", "Description", leaveApplication.StatusId);
            return View(leaveApplication);
        }

        // GET: LeaveApplications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leaveApplication == null)
            {
                return NotFound();
            }

            return View(leaveApplication);
        }

        // POST: LeaveApplications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leaveApplication = await _context.LeaveApplications.FindAsync(id);
            if (leaveApplication != null)
            {
                _context.LeaveApplications.Remove(leaveApplication);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> RejectLeave(int id)
        {
            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leaveApplication == null)
            {
                return NotFound();
            }

            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.systemCode).Where(y => y.systemCode.Code == "LeaveDuration"), "Id", "Description");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");

            return View(leaveApplication);
        }

        [HttpPost]
        public async Task<IActionResult> RejectLeave(LeaveApplication leave)
        {
            var rejectStatus = _context.SystemCodeDetails
                .Include(x => x.systemCode)
                .Where(y => y.systemCode.Code == "LeaveApprovalStatus" &&
                            y.Code == "Rejected").FirstOrDefault();

            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .FirstOrDefaultAsync(m => m.Id == leave.Id);
            if (leaveApplication == null)
            {
                return NotFound();
            }

            leaveApplication.ApprovedOn = DateTime.Now;
            leaveApplication.ApprovedById = "Esther Sui";
            leaveApplication.StatusId = rejectStatus!.Id;
            leaveApplication.ApprovalNotes = leave.ApprovalNotes;

            _context.Update(leaveApplication);
            await _context.SaveChangesAsync();

            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails
                .Include(x => x.systemCode).Where(y => y.systemCode.Code == "LeaveDuration"), "Id", "Description");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");

            return RedirectToAction(nameof(Index));
        }
              
        private bool LeaveApplicationExists(int id)
        {
            return _context.LeaveApplications.Any(e => e.Id == id);
        }
    }
}
