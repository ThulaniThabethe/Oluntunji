using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PaymentController : BaseController
    {
        // GET: Payment/SavedCards
        public ActionResult SavedCards()
        {
            var currentUser = CurrentUser;
            var savedCards = Db.SavedCards
                .Where(sc => sc.UserId == currentUser.UserId)
                .OrderByDescending(sc => sc.IsDefault)
                .ThenBy(sc => sc.CardholderName)
                .ToList();

            var model = new SavedCardsListViewModel
            {
                SavedCards = savedCards.Select(sc => new SavedCardViewModel
                {
                    CardId = sc.CardId,
                    CardholderName = sc.CardholderName,
                    MaskedCardNumber = "**** **** **** " + sc.LastFourDigits,
                    ExpiryDate = sc.ExpiryDate,
                    CardType = sc.CardType,
                    IsDefault = sc.IsDefault,
                    IsActive = sc.IsActive,
                    DateAdded = sc.DateAdded
                }).ToList(),
                TotalCards = savedCards.Count
            };

            return View(model);
        }

        // GET: Payment/AddCard
        public ActionResult AddCard()
        {
            var model = new AddCardViewModel();
            return View(model);
        }

        // POST: Payment/AddCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCard(AddCardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = CurrentUser;

            // Create new saved card
            var savedCard = new SavedCard
            {
                UserId = currentUser.UserId,
                CardholderName = model.CardholderName,
                LastFourDigits = model.CardNumber.Substring(model.CardNumber.Length - 4), // Store only last 4 digits
                ExpiryDate = new DateTime(model.ExpiryYear, model.ExpiryMonth, 1),
                CardType = "visa", // Default card type, should be detected from card number
                IsDefault = model.SetAsDefault,
                DateAdded = DateTime.Now
            };

            // If this is the first card or marked as default, set it as default
            var existingCards = Db.SavedCards.Where(sc => sc.UserId == currentUser.UserId).ToList();
            if (!existingCards.Any() || model.SetAsDefault)
            {
                // Remove default from other cards
                foreach (var card in existingCards.Where(c => c.IsDefault))
                {
                    card.IsDefault = false;
                }
                savedCard.IsDefault = true;
            }

            Db.SavedCards.Add(savedCard);
            Db.SaveChanges();

            TempData["Success"] = "Card added successfully!";
            return RedirectToAction("SavedCards");
        }

        // GET: Payment/EditCard/5
        public ActionResult EditCard(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("SavedCards");
            }

            var currentUser = CurrentUser;
            var savedCard = Db.SavedCards
                .FirstOrDefault(sc => sc.CardId == id && sc.UserId == currentUser.UserId);

            if (savedCard == null)
            {
                return HttpNotFound();
            }

            var model = new EditCardViewModel
            {
                CardId = savedCard.CardId,
                CardholderName = savedCard.CardholderName,
                ExpiryMonth = savedCard.ExpiryDate.Month,
                ExpiryYear = savedCard.ExpiryDate.Year,
                CardType = savedCard.CardType,
                IsDefault = savedCard.IsDefault,
                IsActive = savedCard.IsActive,
                AvailableCardTypes = GetCardTypes()
            };

            return View(model);
        }

        // POST: Payment/EditCard/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCard(EditCardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCardTypes = GetCardTypes();
                return View(model);
            }

            var currentUser = CurrentUser;
            var savedCard = Db.SavedCards
                .FirstOrDefault(sc => sc.CardId == model.CardId && sc.UserId == currentUser.UserId);

            if (savedCard == null)
            {
                return HttpNotFound();
            }

            // Update card details
            savedCard.CardholderName = model.CardholderName;
            savedCard.ExpiryDate = new DateTime(model.ExpiryYear, model.ExpiryMonth, 1);
            savedCard.CardType = model.CardType;

            // Handle default card setting
            if (model.IsDefault && !savedCard.IsDefault)
            {
                // Remove default from other cards
                var otherCards = Db.SavedCards
                    .Where(sc => sc.UserId == currentUser.UserId && sc.CardId != model.CardId)
                    .ToList();

                foreach (var card in otherCards.Where(c => c.IsDefault))
                {
                    card.IsDefault = false;
                }
                savedCard.IsDefault = true;
            }

            Db.SaveChanges();

            TempData["Success"] = "Card updated successfully!";
            return RedirectToAction("SavedCards");
        }

        // POST: Payment/SetDefaultCard
        [HttpPost]
        public ActionResult SetDefaultCard(int cardId)
        {
            try
            {
                var currentUser = CurrentUser;
                var savedCard = Db.SavedCards
                    .FirstOrDefault(sc => sc.CardId == cardId && sc.UserId == currentUser.UserId);

                if (savedCard == null)
                {
                    return Json(new { success = false, message = "Card not found." });
                }

                // Remove default from all other cards
                var otherCards = Db.SavedCards
                    .Where(sc => sc.UserId == currentUser.UserId && sc.CardId != cardId)
                    .ToList();

                foreach (var card in otherCards)
                {
                    card.IsDefault = false;
                }

                // Set this card as default
                savedCard.IsDefault = true;
                Db.SaveChanges();

                return Json(new { success = true, message = "Card set as default successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error setting default card: " + ex.Message });
            }
        }

        // POST: Payment/DeleteCard
        [HttpPost]
        public ActionResult DeleteCard(int cardId)
        {
            try
            {
                var currentUser = CurrentUser;
                var savedCard = Db.SavedCards
                    .FirstOrDefault(sc => sc.CardId == cardId && sc.UserId == currentUser.UserId);

                if (savedCard == null)
                {
                    return Json(new { success = false, message = "Card not found." });
                }

                // Check if this is the last card
                var userCards = Db.SavedCards.Where(sc => sc.UserId == currentUser.UserId).ToList();
                if (userCards.Count == 1)
                {
                    return Json(new { success = false, message = "Cannot delete your only saved card. Please add another card first." });
                }

                // If this was the default card, set another card as default
                if (savedCard.IsDefault)
                {
                    var nextCard = userCards.FirstOrDefault(sc => sc.CardId != cardId);
                    if (nextCard != null)
                    {
                        nextCard.IsDefault = true;
                    }
                }

                Db.SavedCards.Remove(savedCard);
                Db.SaveChanges();

                return Json(new { success = true, message = "Card deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting card: " + ex.Message });
            }
        }

        // GET: Payment/AdminSavedCards - Admin view for all saved cards
        [Authorize(Roles = "Admin")]
        public ActionResult AdminSavedCards()
        {
            try
            {
                var savedCards = Db.SavedCards
                    .Include(sc => sc.User)
                    .OrderByDescending(sc => sc.DateAdded)
                    .ToList();

                var model = new AdminSavedCardsListViewModel
                {
                    SavedCards = savedCards.Select(sc => new AdminSavedCardViewModel
                    {
                        CardId = sc.CardId,
                        CardHolderName = sc.CardholderName,
                        CardNumber = sc.LastFourDigits,
                        ExpiryMonth = sc.ExpiryDate.Month,
                        ExpiryYear = sc.ExpiryDate.Year,
                        CardType = sc.CardType,
                        IsDefault = sc.IsDefault,
                        CreatedDate = sc.DateAdded,
                        UserId = sc.UserId,
                        UserName = sc.User?.FullName ?? "Unknown User",
                        UserEmail = sc.User?.Email ?? "Unknown Email"
                    }).ToList(),
                    TotalCards = savedCards.Count
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error in AdminSavedCards: {ex.Message}");
                TempData["Error"] = "An error occurred while loading saved cards. Please try again.";
                return RedirectToAction("AdminDashboard", "Profile");
            }
        }

        private List<CardTypeViewModel> GetCardTypes()
        {
            return new List<CardTypeViewModel>
            {
                new CardTypeViewModel { CardType = "visa", CardTypeDisplayName = "Visa", IsSupported = true },
                new CardTypeViewModel { CardType = "mastercard", CardTypeDisplayName = "Mastercard", IsSupported = true },
                new CardTypeViewModel { CardType = "amex", CardTypeDisplayName = "American Express", IsSupported = true },
                new CardTypeViewModel { CardType = "discover", CardTypeDisplayName = "Discover", IsSupported = false }
            };
        }
    }
}