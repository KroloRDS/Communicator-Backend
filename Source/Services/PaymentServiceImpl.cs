using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.IO;
using System;

using Communicator.Source.DTOs.JSONs;
using Communicator.HelperClasses;
using Communicator.Entities;
using Communicator.DTOs;

namespace Communicator.Services
{
	public class PaymentServiceImpl : IPaymentService
	{
		private readonly CommunicatorDbContex _context;
		private readonly string _currency = "PLN";

		public PaymentServiceImpl(CommunicatorDbContex context)
		{
			_context = context;
		}

		public byte[] MakePayment(string requestName, JToken data, int userId)
		{
			var card = data.SelectToken("card");
			var request = new PaymentRequest
			{
				Amount = data.Value<float>("amount"),
				CardCode = card.Value<int>("cvv"),
				CardNumber = card.Value<long>("number"),
				ExpirationDate = card.Value<string>("expirationDate")
			};

			var response = AddPayment(userId, request);

			if (!response.Response.Equals(Error.OK))
			{
				return new JsonResponse(requestName, response).GetBytes();
			}

			var authorizeNetResponse = SendAuthorizeNetRequest(request);
			if (!authorizeNetResponse.Equals(Error.OK))
			{
				UpdateStatus(response.ID, false);
				return new JsonResponse(requestName, authorizeNetResponse).GetBytes();
			}
			return new JsonResponse(requestName, UpdateStatus(response.ID, true)).GetBytes();
		}

		private PaymentResponse AddPayment(int userId, PaymentRequest request)
		{
			var payment = new PaymentEntity
			{
				Amount = request.Amount,
				Currency = _currency,
				DateTime = DateTime.UtcNow,
				Status = (int)PaymentEntity.Statuses.PENDING,
				UserID = userId
			};

			_context.PaymentEntity.Add(payment);
			_context.SaveChanges();

			return new PaymentResponse
			{
				ID = payment.ID,
				Response = Error.OK
			};
		}

		private string UpdateStatus(int id, bool status)
		{
			var payment = _context.PaymentEntity.FirstOrDefault(x => x.ID == id);
			if (payment == null)
			{
				return string.Format(Error.CANNOT_FIND_PAYMENT, id);
			}

			payment.Status = status ? (int)PaymentEntity.Statuses.SUCCEDED : (int)PaymentEntity.Statuses.FAILED;
			_context.SaveChanges();
			return Error.OK;
		}

		private static string SendAuthorizeNetRequest(PaymentRequest request)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://apitest.authorize.net/xml/v1/request.api");
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				streamWriter.Write(JObject.FromObject(CreateAuthorizeNetRequest(request)).ToString());
			}

			using var streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream());
			var response = JObject.Parse(streamReader.ReadToEnd());

			return response.SelectToken("messages").Value<string>("resultCode").Equals("Error") ?
				response.SelectToken("messages.message").First.Value<string>("text") : Error.OK;
		}

		private static AuthorizeNetRequest CreateAuthorizeNetRequest(PaymentRequest request)
		{
			return new AuthorizeNetRequest
			{
				createTransactionRequest = new CreateTransactionRequest
				{
					merchantAuthentication = new MerchantAuthentication
					{
						name = "3mK8nGVR2Pc",
						transactionKey = "3G26PhZAX82f44pF"
					},
					transactionRequest = new TransactionRequest
					{
						transactionType = "authCaptureTransaction",
						amount = request.Amount,
						payment = new Payment
						{
							creditCard = new CreditCard
							{
								cardNumber = request.CardNumber,
								expirationDate = request.ExpirationDate,
								cardCode = request.CardCode
							}
						}
					}
				}
			};
		}
	}
}
