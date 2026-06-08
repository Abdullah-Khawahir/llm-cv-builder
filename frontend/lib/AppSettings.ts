type AppSettingsType = {
  API_Base_URL: string;
};

const AppSettings: AppSettingsType = {
  API_Base_URL: process.env.NEXT_PUBLIC_API_URL!,
};

export default AppSettings;
