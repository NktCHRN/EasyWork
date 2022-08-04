import { HttpHeaders, HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { AddBanModel } from '../shared/ban/add-ban.model';
import { BannedModel } from '../shared/user/banned.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class BanService extends BaseService {

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService,
    private _tokenService: TokenService) {
    super();
  }

  override serviceBaseURL: string = this.baseURL + 'Bans/';

  public add(model: AddBanModel): Observable<BannedModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.post<BannedModel>(this.serviceBaseURL, model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}
