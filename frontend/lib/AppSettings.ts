
type AppSettingsType = {
  API_Base_URL: string;
  MERCHANT_ID: string;
  TAP_KEY: string;
};

const AppSettings: AppSettingsType = {
  API_Base_URL: process.env.NEXT_PUBLIC_API_URL!,
  MERCHANT_ID: process.env.NEXT_PUBLIC_MERCHANT_ID!,
  TAP_KEY: process.env.NEXT_PUBLIC_TAP_KEY!,
};

export default AppSettings;
