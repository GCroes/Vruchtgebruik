import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
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

  validationMessages = {
    assetValue: [
      { type: 'required', message: 'Asset value is required.' },
      { type: 'min', message: 'Must be greater than 0.' }
    ],
    age: [
      { type: 'required', message: 'Age is required.' },
      { type: 'min', message: 'Must be greater than 0.' }
    ],
    sex: [
      { type: 'required', message: 'Sex is required.' }
    ],
    factorMethod: [
      { type: 'required', message: 'Factor method is required.' }
    ]
  };

  constructor(private fb: FormBuilder, private calculateService: CalculateService) {
    this.calcForm = this.fb.group({
      assetValue: [null, [Validators.required, Validators.min(1)]],
      age: [null, [Validators.required, Validators.min(1)]],
      sex: ['', Validators.required],
      factorMethod: ['', Validators.required]
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

    const req: CalculationRequest = this.calcForm.value;

    this.calculateService.calculate(req).subscribe({
      next: (response) => {
        console.log('API response:', response);
        this.calcResult = response.response;
      },
      error: (error) => {
        if (error.error?.errors) {
          // Handle server validation errors array
          this.apiError = error.error.errors.map((e: any) => e.errorMessage || e.message).join('; ');
        } else if (error.error?.error) {
          // Handle single server error (e.g., { error: "some error" })
          this.apiError = error.error.error;
        } else {
          this.apiError = "An error occurred while calculating. Please try again.";
        }
      }
    });
  }
}
