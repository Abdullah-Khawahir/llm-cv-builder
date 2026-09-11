"use client"
import { api } from "@/app/ApiClient";
import TapCardComponent from "@/components/tab/TapCardComponent";
import AppSettings from "@/lib/AppSettings";

export default function CheckoutPage() {

  return (
    <div>
      <h1>Checkout</h1>

      <TapCardComponent
        publicKey={AppSettings.TAP_KEY}
        merchantId={AppSettings.MERCHANT_ID}
        amount={100}
        currency="SAR"
        customer={
          // authed user info
          {
            id: "user_123",
            firstName: "Abdullah M",
            lastName: "Alkhwahir",
            email: "abdullah.khawahir@gmail.com",
            phoneCountryCode: "966",
            phoneNumber: "534531781",
          }
        }
        onError={e => console.log(e)}
        onSuccess={async (data) => {
          try {
            const response = await api.paymentsCreate({
              token: data.id,
              amount: 100,
              currency: "SAR",
              email: data.customer.email,
              provider: "TAP",
              firstName: data.customer.firstName
            })

            if (response.status >= 400) {
              console.error("Backend error:", response.statusText);
              return;
            }

            const result = response.data;

            if (result.redirectUrl) {
              window.location.href = result.redirectUrl
            }

          } catch (error) {
            console.error("Payment error:", error);
          }
        }
        }
      />
    </div>
  );
}
