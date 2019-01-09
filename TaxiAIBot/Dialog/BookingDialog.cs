using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TaxiAIBot.Common;
using TaxiAIBot.Model;
using Newtonsoft.Json;
using TaxiAIBot.APIs;

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
                    name = name.ToString(),
                    date = DateTime.Now,
                    address = new Address() {
                        lat = "Address LAT",
                        lng = "Address LNG",
                        formatted = address.ToString()
                    },
                    destination = new Destination() {
                        lat = "Destination LAT",
                        lng = "Destination LNG",
                        formatted = address.ToString()
                    }, 
                    phone = phone.ToString(), 
                    payment = new Payment() {
                        cost = 2f,
                        price = 1.5f,
                        payment_fixed = 1.0f
                    }
                };
                var booking = stepContext.Values[StringHelper.PROMPT_BOOKING] as Booking; 
                //For Test
                var b =  VehicleAPI.CreateBooking<Booking,Booking>("https://api.icabbi.com/v2/accountfields/generic",  booking);

              //  APIResponse res = VehicleAPI.CreateBooking<Booking, Booking>("https://api.icabbi.com/v2/accountfields/generic", booking ); 
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{booking.name} has created a book from { booking.address.formatted} to  {booking.destination.formatted} at {booking.date.ToString()}"), cancellationToken);
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
