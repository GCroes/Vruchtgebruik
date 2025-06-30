import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { CalculateService } from './services/calculate.service'; // Adjust path as needed
import { CalculationRequest } from './interfaces/calculation-request'; // Adjust path as needed
import { CalculationResponse } from './interfaces/calculation-response';
import { ApiResult } from './interfaces/api-result';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  
  calcForm: FormGroup;
  calcResult: CalculationResponse | null = null;
  apiError: string | null = null;
  loading = false;

  validationMessages = {
    assetValue: [
      { type: 'required', message: 'Eigendomswaarde is verplicht.' },
      { type: 'min', message: 'De waard moet groter zijn dan 0.' }
    ],
    age: [
      { type: 'required', message: 'Leeftijd is verplicht.' },
      { type: 'min', message: 'moet groter zijn dan 0.' }
    ],
    sex: [
      { type: 'required', message: 'Geslacht is verplicht.' }
    ],
    factorMethod: [
      { type: 'required', message: 'Berekeningsmethode is verplicht.' }
    ]
  };

  constructor(private fb: FormBuilder, private calculateService: CalculateService) {

    this.calcForm = this.fb.group({
      assetValue: new FormControl("", [Validators.required, Validators.min(1)]),
      age: new FormControl("", [Validators.required, Validators.min(1)]),
      sex: new FormControl("", Validators.required),
      factorMethod: new FormControl("", Validators.required)
    });
  }

  ngOnInit(): void { throw new Error("Not implemented"); }

  onSubmit() {
    if (this.calcForm.invalid) {
      this.calcForm.markAllAsTouched();
      return;
    }

    this.apiError = null;
    this.calcResult = null;
    this.loading = true;

    const req: CalculationRequest = this.calcForm.value;

    this.calculateService.calculate(req).subscribe({
      next: (response) => {
        console.log('API response:', response);
        this.calcResult = response.response;
        this.loading = false;
      },
      error: (error) => {
        if (error.error?.errors && typeof error.error.errors === 'object') {
          // ProblemDetails validation errors
          const fieldErrors = Object.entries(error.error.errors as { [key: string]: string[] })
            .map(([field, messages]) => `${field}: ${(messages as string[]).join(', ')}`)
            .join('; ');
          this.apiError = fieldErrors || error.error.title || "Validation error occurred.";
        } else if (error.error?.error) {
          // Legacy or custom error property
          this.apiError = error.error.error;
        } else if (error.error?.title) {
          // Generic ProblemDetails
          this.apiError = error.error.title;
        } else {
          this.apiError = "An error occurred while calculating. Please try again.";
        }
        this.loading = false;
      }
    });
  }
}
