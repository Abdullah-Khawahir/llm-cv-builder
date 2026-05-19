import { NextAuthOptions } from "next-auth";
import GitHubProvider from "next-auth/providers/github";

export const authOptions: NextAuthOptions = {
    providers: [
        GitHubProvider({
            clientId: process.env.GITHUB_ID!,
            clientSecret: process.env.GITHUB_SECRET!,
        }),
    ],
    session: {
        strategy: "jwt", // important for server usage
    },
    callbacks: {
        async session({ session, token }) {
            if (token) {
                session.user.id = token.sub; // attach user id
            }
            return session;
        },
    },
};
