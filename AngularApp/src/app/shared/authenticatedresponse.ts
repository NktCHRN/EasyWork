import { TokenResponse } from "./tokenresponse";

export interface AuthenticatedResponse{
  isAuthSuccessful: boolean;
  errorMessage: string;
  token: TokenResponse | null | undefined;
  errorDetails: object | null | undefined;
}