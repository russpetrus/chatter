using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using chatter.Models;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;

namespace chatter.Controllers
{
    public class ChatsController : Controller
    {

       
        private ChatterEntities db = new ChatterEntities();

        // GET: Chats

        
       [RequireHttps]
       [Authorize]
        public ActionResult Index()
        {
            
            var chats = db.Chats.Include(c => c.AspNetUser);
            return View(chats.ToList());
        }

        
        public JsonResult TestJson()
        {
            //This is my original SQL query
            //SELECT AspNetUsers.UserName,  Chat.Message
            //from Chat
            //INNER JOIN AspNetUsers ON Chat.UserId = AspNetUsers.Id
            //ORDER BY Chat.TimeStamp DESC;

            //LINQ appears below
            var chats = from Chats in db.Chats
                        orderby
                          Chats.TimeStamp descending
                        select new
                        {
                            Chats.AspNetUser.UserName,
                            Chats.Message
                        };

            var output = JsonConvert.SerializeObject(chats.ToList());
            return Json(output, JsonRequestBehavior.AllowGet);

        }


        // GET: Chats/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chat chat = db.Chats.Find(id);
            if (chat == null)
            {
                return HttpNotFound();
            }
            return View(chat);
        }

        // GET: Chats/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: Chats/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,UserId,Message,TimeStamp")] Chat chat)
        {
            if (ModelState.IsValid)
            {
                db.Chats.Add(chat);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", chat.UserId);
            return View(chat);
        }

        public JsonResult PostChats([Bind(Include = "Message")] Chat chat)
        {

            //PostChats method knows to expect the message from chats when chats view .post method sends the information

            //Timestamp is set to DateTime.Now using the control instead of on the chats form in chats view
            chat.TimeStamp = DateTime.Now;
            //Now, since we have a foreign key join on the aspnetuser table, the 2 models in EF (AspNetUser and Chat) reference each other
            // Because of this, we need to get a complete user object, not just the userID 
            string currentUserId = User.Identity.GetUserId();
            chat.AspNetUser = db.AspNetUsers.FirstOrDefault(x => x.Id == currentUserId);


            if (ModelState.IsValid)
            {
                db.Chats.Add(chat);
                db.SaveChanges();
            }
            return new JsonResult() { Data = JsonConvert.SerializeObject(chat.ID), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



        // GET: Chats/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chat chat = db.Chats.Find(id);
            if (chat == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", chat.UserId);
            return View(chat);
        }

        // POST: Chats/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UserId,Message,TimeStamp")] Chat chat)
        {
            if (ModelState.IsValid)
            {
                db.Entry(chat).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", chat.UserId);
            return View(chat);
        }

        // GET: Chats/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chat chat = db.Chats.Find(id);
            if (chat == null)
            {
                return HttpNotFound();
            }
            return View(chat);
        }

        // POST: Chats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Chat chat = db.Chats.Find(id);
            db.Chats.Remove(chat);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
