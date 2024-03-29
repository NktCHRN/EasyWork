import { Injectable } from '@angular/core';
import { baseURL } from '../shared/constants/baseurl';

@Injectable({
  providedIn: 'root'
})
export abstract class BaseService {

  constructor() { }

  baseURL: string = baseURL;

  abstract serviceBaseURL: string;
}
