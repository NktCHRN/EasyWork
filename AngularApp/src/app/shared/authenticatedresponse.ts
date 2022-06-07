import { TokenResponse } from "./tokenresponse";

export interface AuthenticatedResponse{
  token: TokenResponse | null | undefined;
}