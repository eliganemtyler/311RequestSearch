import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { of } from 'rxjs';
import { map, catchError, filter, mapTo, tap } from 'rxjs/operators';
import { GettingStarted } from './getting-started.model';

@Injectable({
  providedIn: 'root'
})
export class GettingStartedService {
  constructor(private http: HttpClient) {}

  getAppName() {
    return this.http.get<GettingStarted>('api/GettingStarted').pipe(
      map(gettingStarted => gettingStarted.applicationName),
      catchError((error: HttpErrorResponse) => of(error).pipe(
        filter(err => err.status === 401),
        tap(() => console.warn('401 Unauthorized to access GettingStarted resource.\nPlease "sign in" to update the title.')),
        mapTo(null)
      ))
    );
  }
}
