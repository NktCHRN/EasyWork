import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
  HTTP_INTERCEPTORS
} from '@angular/common/http';
import { BehaviorSubject, catchError, filter, Observable, throwError, switchMap, take, from } from 'rxjs';
import { TokenService } from '../services/token.service';
import { AccountService } from '../services/account.service';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { TokenResponseModel } from '../shared/token/token-response.model';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);
  constructor(private _tokenService: TokenService, private _accountService: AccountService, private _router: Router, 
    private _dialog: MatDialog) { }
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<Object>> {
    let authReq = req;
    const token = this._tokenService.getJwtToken();
    if (token) {
      authReq = this.addTokenHeader(req, token);
    }
    return next.handle(authReq).pipe(catchError(error => {
      if (error instanceof HttpErrorResponse 
        && !authReq.url.toLowerCase().includes('login') 
        && error.status === 401) {
        return this.handle401Error(authReq, next);
      }
      else if (error instanceof HttpErrorResponse 
        && error.status === 403 && error?.error === "You are banned") {
        return this.handleBannedError(error);
      }
      else if (error instanceof HttpErrorResponse && error.status === 404)
      {
        return this.handle404Error(error);
      }
      return throwError(() => error);
    }));
  }

  private handle404Error(error: HttpErrorResponse) : Observable<HttpEvent<any>> {
    this._router.navigate(["**"], {skipLocationChange: true}); 
    console.log(error);
    return throwError(() => error);
  }

  private handleBannedError(error: HttpErrorResponse) : Observable<HttpEvent<any>> {
    this._accountService.logout();
    this._accountService.sendAuthStateChangeNotification(false);
    this._dialog.closeAll();
    this._router.navigate(["login"]); 
    console.log(error);
    return throwError(() => error);
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler) {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);
        return from(this._tokenService.refreshToken())
        .pipe(
          switchMap(inner => {
            return inner.pipe(
            switchMap((token: TokenResponseModel) => {
              this.isRefreshing = false;
              this.refreshTokenSubject.next(token.accessToken);
              this._accountService.sendAuthStateChangeNotification(true);
              return next.handle(this.addTokenHeader(request, token.accessToken));
            }),
            catchError((err) => {
              this.isRefreshing = false;
              this._accountService.logout();
              this._accountService.sendAuthStateChangeNotification(false);
              this._dialog.closeAll();
              this._router.navigate(["login"]); 
              console.log(err);
              return throwError(() => err);
            }));
          })
        );
    }
    return this.refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap((token) => next.handle(this.addTokenHeader(request, token)))
    );
  }
  private addTokenHeader(request: HttpRequest<any>, token: string) {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }
}

export const authInterceptor = [
  { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
];
