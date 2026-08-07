import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiConfigService {
  readonly apiUrl = environment.apiUrl;
  readonly baseUrl = environment.apiUrl.replace('/api', '');
  readonly uploadsUrl = `${this.baseUrl}/Uploads`;

  url(path: string): string {
    return `${this.apiUrl}/${path.replace(/^\//, '')}`;
  }

  uploadPath(imagePath?: string | null): string {
    if (!imagePath) {
      return 'assets/placeholder-product.png';
    }
    if (imagePath.startsWith('http')) {
      return imagePath;
    }
    return `${this.baseUrl}/${imagePath.replace(/\\/g, '/')}`;
  }
}
