type AppSettingsType = {
  API_URL: string;
};

const AppSettings: AppSettingsType = {
  API_URL: process.env.NEXT_PUBLIC_API_URL!,
};

export default AppSettings;
