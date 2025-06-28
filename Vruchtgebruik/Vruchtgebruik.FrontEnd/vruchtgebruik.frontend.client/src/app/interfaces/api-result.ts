import { CalculationResponse } from './calculation-response';

export interface ApiResult {
  correlationId: string;
  response: CalculationResponse;
}
