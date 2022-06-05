import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { GeneralInfo } from '../shared/generalinfo';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class GeneralinfoService extends BaseService {

  constructor(private http: HttpClient,
    private processHTTPMsgService: ProcessHTTPMsgService) {
    super();
  }

  override serviceBaseURL: string = this.baseURL + 'GeneralInfo/';

  public getInfo(): Observable<GeneralInfo>
  {
    return this.http.get<GeneralInfo>(this.serviceBaseURL)
    .pipe(catchError(this.processHTTPMsgService.handleError))
  }
}
