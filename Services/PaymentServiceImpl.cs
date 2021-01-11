using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.IO;
using System;

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

		public PaymentResponse Add(int userId, PaymentRequest request)
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
				Response = ErrorCodes.OK
			};
		}

		public string UpdateStatus(int id, bool status)
		{
			var payment = _context.PaymentEntity.FirstOrDefault(x => x.ID == id);
			if (payment == null)
			{
				return string.Format(ErrorCodes.CANNOT_FIND_PAYMENT, id);
			}

			payment.Status = status ? (int)PaymentEntity.Statuses.SUCCEDED : (int)PaymentEntity.Statuses.FAILED;
			_context.SaveChanges();
			return ErrorCodes.OK;
		}

		public string SendAuthorizeNetRequest(PaymentRequest request)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://apitest.authorize.net/xml/v1/request.api");
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				streamWriter.Write(CreateAuthorizeNetRequest(request).ToString());
			}

			using var streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream());
			var response = JObject.Parse(streamReader.ReadToEnd());

			return response.SelectToken("transactionResponse").Value<string>("responseCode").Equals("1") &&
				response.SelectToken("messages").Value<string>("resultCode").Equals("Ok") ?
				ErrorCodes.OK : response.SelectToken("errors").Values().First().Value<string>("errorText");
		}

		private static JObject CreateAuthorizeNetRequest(PaymentRequest request)
		{
			return new JObject
			{
				{ "createTransactionRequest", new JObject
				{
					{ "merchantAuthentication", new JObject
					{
						{ "name", "3mK8nGVR2Pc" },
						{ "transactionKey", "3G26PhZAX82f44pF" },
					}
					},
					{ "transactionRequest", new JObject
					{
						{ "transactionType", "authCaptureTransaction" },
						{ "amount", request.Amount.ToString() },
						{ "payment", new JObject
						{
							{ "creditCard", new JObject
							{
								{ "cardNumber", request.CardNumber.ToString() },
								{ "expirationDate", request.ExpirationDate.ToString() },
								{ "cardCode", request.CardCode.ToString() },
							}
							},
						}
						},
					}
					},
				}
				},
			};
		}
	}
}
