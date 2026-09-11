"use client";

import { useEffect, useMemo, useRef } from "react";

type Props = {
  publicKey: string;
  merchantId: string;
  amount: number;
  currency: string;

  customer: {
    id?: string;
    firstName: string;
    lastName: string;
    email: string;
    phoneCountryCode: string;
    phoneNumber: string;
  };

  onSuccess: (data: any) => void;
  onError?: (error: any) => void;
};

declare global {
  interface Window {
    CardSDK: any;
  }
}

let scriptPromise: Promise<void> | null = null;

function loadTapScript() {
  if (window.CardSDK) return Promise.resolve();

  if (scriptPromise) return scriptPromise;

  scriptPromise = new Promise<void>((resolve, reject) => {
    const script = document.createElement("script");
    script.src = "https://tap-sdks.b-cdn.net/card/1.0.2/index.js";
    script.async = true;

    script.onload = () => resolve();
    script.onerror = reject;

    document.body.appendChild(script);
  });

  return scriptPromise;
}

export default function TapCardComponent({
  publicKey,
  merchantId,
  amount,
  currency,
  customer,
  onSuccess,
  onError,
}: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const unmountRef = useRef<null | (() => void)>(null);
  const tokenizeRef = useRef<null | (() => void)>(null);

  const memoCustomer = useMemo(() => customer, [
    customer.firstName,
    customer.lastName,
    customer.email,
    customer.phoneCountryCode,
    customer.phoneNumber,
    customer.id,
  ]);

  useEffect(() => {
    let isMounted = true;

    const init = async () => {
      try {
        await loadTapScript();

        if (!isMounted || !containerRef.current) return;

        const {
          renderTapCard,
          Theme,
          Currencies,
          Direction,
          Edges,
          Locale,
        } = window.CardSDK;

        const result = renderTapCard(containerRef.current, {
          publicKey,
          merchant: {
            id: merchantId,
          },
          transaction: {
            amount,
            currency: Currencies[currency as keyof typeof Currencies],
          },
          customer: {
            name: [
              {
                lang: Locale.EN,
                first: memoCustomer.firstName,
                last: memoCustomer.lastName,
                middle: "",
              },
            ],
            nameOnCard: `${memoCustomer.firstName} ${memoCustomer.lastName}`,
            editable: true,
            contact: {
              email: memoCustomer.email,
              phone: {
                countryCode: memoCustomer.phoneCountryCode,
                number: memoCustomer.phoneNumber,
              },
            },
          },
          acceptance: {
            supportedBrands: ["VISA", "MASTERCARD", "MADA"],
            supportedCards: "ALL",
          },
          fields: {
            cardHolder: true,
          },
          addons: {
            displayPaymentBrands: true,
            loader: true,
            saveCard: true,
          },
          interface: {
            locale: Locale.EN,
            theme: Theme.LIGHT,
            edges: Edges.CURVED,
            direction: Direction.LTR,
          },

          // onReady: () => { console.log("Tap Card ready"); },
          // onFocus: () => { console.log("Tap Card focus"); },
          // onBinIdentification: (d: any) => { console.log("BIN Identification:", d); },
          // onValidInput: (d: any) => { console.log("Valid input:", d); },
          // onInvalidInput: (d: any) => { console.log("Invalid input:", d); },

          // onChangeSaveCardLater: (v: boolean) => {
          //   console.log("Save card selected:", v);
          // },

          onError: (err: any) => {
            console.error("Tap SDK error:", err);
            onError?.(err);
          },

          onSuccess: (data: any) => {
            console.log("Tap SDK payment success:", data);
            // Ensure we pass the token ID to the parent onSuccess handler
            onSuccess?.({ id: data.id, customer });
          },
        });

        // SDK instance handlers (IMPORTANT FIX)
        unmountRef.current = result?.unmount;
        tokenizeRef.current = result?.tokenize;
      } catch (err) {
        console.error("Tap SDK failed to load or initialize:", err);
        onError?.(err);
      }
    };

    init();


    return () => {
      isMounted = false;
      unmountRef.current?.();
      unmountRef.current = null;
      tokenizeRef.current = null;
    };
  }, [
    publicKey,
    merchantId,
    amount,
    currency,
    memoCustomer,
    onSuccess,
    onError,
  ]);

  return (
    <div className="flex flex-col w-full gap-4">
      <div id="tap-card-container" ref={containerRef} />

      <button
        onClick={() => window.CardSDK.tokenize()}
        className="px-4 py-2 bg-black text-white rounded">
        Pay
      </button>
    </div>
  );
}
