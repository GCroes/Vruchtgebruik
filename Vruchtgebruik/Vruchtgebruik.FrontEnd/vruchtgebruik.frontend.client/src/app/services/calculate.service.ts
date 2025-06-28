import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CalculationRequest } from "../interfaces/calculation-request";;
import { ApiResult } from "../interfaces/api-result";

@Injectable({
  providedIn: 'root'
})
export class CalculateService {
  private apiUrl = environment.apiUrl + '/Calculate';

  constructor(private http: HttpClient) { }

  calculate(request: CalculationRequest): Observable<ApiResult> {
    return this.http.post<ApiResult>(this.apiUrl, request);
  }
}
