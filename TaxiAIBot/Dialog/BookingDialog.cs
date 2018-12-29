using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TaxiAIBot.Common;
using TaxiAIBot.Model;

namespace TaxiAIBot.Dialog
{
    public static class BookingDialog
    {
        public static async Task<DialogTurnResult> CreateBookingDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[StringHelper.PROMPT_BOOKING] = new Booking();
            return await stepContext.BeginDialogAsync(StringHelper.DIA_CREATE_BOOKING, null, cancellationToken);
        }


        public static async Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        { 
            if (stepContext.Result is IDictionary<string, object> result && result.Count > 0)
            {
              
                var name = result[StringHelper.PROMPT_BOOKING_NAME];
                var phone = result[StringHelper.PROMPT_BOOKING_PHONE];
                var address = result[StringHelper.PROMPT_BOOKING_ADDRESS];
                var destination = result[StringHelper.PROMPT_BOOKING_DESTINATION]; 

                stepContext.Values[StringHelper.PROMPT_BOOKING] = new Booking()
                {
                    Name = name.ToString(),
                    BookTime = DateTime.Now,
                    Address = new GPS() {
                        Lat = "Address LAT",
                        Lng = "Address LNG",
                        Formatted = address.ToString()
                    },
                    Destination = new GPS() {
                        Lat = "Destination LAT",
                        Lng = "Destination LNG",
                        Formatted = address.ToString()
                    }, 
                    Phone = phone.ToString(), 
                    Payment = new Payment() {
                        Cost = 1.2f,
                        Price = 1.5f,
                        Fixed = 0
                    }
                };
                var booking = stepContext.Values[StringHelper.PROMPT_BOOKING] as Booking;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{booking.Name} has created a book from { booking.Address.Formatted} to  {booking.Destination.Formatted} at {booking.BookTime.ToString()}"), cancellationToken);
            } 
            return await stepContext.EndDialogAsync();
        }



        public static Task<bool> EmailPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var email = promptContext.Recognized.Value;
            string strExp = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            Regex r = new Regex(strExp);
            Match m = r.Match(email); 
            return Task.FromResult(m.Success); 
        }
    }
}
