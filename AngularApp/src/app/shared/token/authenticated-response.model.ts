import { TokenResponseModel } from "./token-response.model";

export interface AuthenticatedResponseModel {
  isAuthSuccessful: boolean;
  errorMessage: string;
  token: TokenResponseModel | null | undefined;
  errorDetails: object | null | undefined;
}
