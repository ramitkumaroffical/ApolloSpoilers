import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { AboutComponent } from './about.component';
import { CatalogService } from '../../core/services/catalog.service';

describe('AboutComponent', () => {
  let mockCatalog: { search: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockCatalog = { search: vi.fn().mockReturnValue(of({ items: [], totalCount: 0 })) };

    await TestBed.configureTestingModule({
      imports: [AboutComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: CatalogService, useValue: mockCatalog },
      ],
    }).compileComponents();
  });

  function create() {
    const fixture = TestBed.createComponent(AboutComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and loads featured products on init', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(mockCatalog.search).toHaveBeenCalledWith(
      expect.objectContaining({ isFeatured: true, pageSize: 6 })
    );
  });

  it('has five spoiler types defined', () => {
    const fixture = create();
    expect(fixture.componentInstance.spoilerTypes.length).toBe(5);
  });
});
