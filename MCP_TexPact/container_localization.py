import os
import requests
from typing import List, Optional, Dict, Union
from pydantic import BaseModel
from dotenv import load_dotenv
from collections import defaultdict

load_dotenv()
API_BASE = os.getenv("API_BASE_URL", "http://localhost:5181/api")


class LocalizationModel(BaseModel):
    id: int
    containerId: int
    sectionId: int
    datetime: str


class ContainerModel(BaseModel):
    containerId: int
    containerName: Union[Dict[str, str], str]


class SectionModel(BaseModel):
    sectionId: int
    name: Union[Dict[str, str], str]


def fetch_container_localizations() -> List[LocalizationModel]:
    resp = requests.get(f"{API_BASE}/ContainerLocalizationHistory")
    resp.raise_for_status()
    return [LocalizationModel(**j) for j in resp.json()]


def fetch_containers() -> List[ContainerModel]:
    resp = requests.get(f"{API_BASE}/Container")
    resp.raise_for_status()
    return [ContainerModel(**j) for j in resp.json()]


def fetch_sections() -> List[SectionModel]:
    resp = requests.get(f"{API_BASE}/PlantFloorSection")
    resp.raise_for_status()
    return [SectionModel(**j) for j in resp.json()]


def fetch_localizations_by_section(section_id: int) -> List[LocalizationModel]:
    all_locs = fetch_container_localizations()
    return [loc for loc in all_locs if loc.sectionId == section_id]


def fetch_localizations_by_container(container_id: int) -> List[LocalizationModel]:
    all_locs = fetch_container_localizations()
    return [loc for loc in all_locs if loc.containerId == container_id]


def fetch_localization_by_id(localization_id: int) -> Optional[LocalizationModel]:
    all_locs = fetch_container_localizations()
    for loc in all_locs:
        if loc.id == localization_id:
            return loc
    return None


def fetch_localizations_with_container_names_by_section(section_id: int) -> List[Dict]:
    locs = fetch_localizations_by_section(section_id)
    containers = {c.containerId: get_name(c.containerName) for c in fetch_containers()}
    return [
        {
            "id": loc.id,
            "containerId": loc.containerId,
            "containerName": containers.get(loc.containerId, "<unknown>"),
            "datetime": loc.datetime
        }
        for loc in locs
    ]


def fetch_sections_for_container(container_id: int) -> List[Dict]:
    locs = fetch_localizations_by_container(container_id)
    sections = {s.sectionId: get_name(s.name) for s in fetch_sections()}
    return [
        {
            "id": loc.id,
            "sectionId": loc.sectionId,
            "sectionName": sections.get(loc.sectionId, "<unknown>"),
            "datetime": loc.datetime
        }
        for loc in locs
    ]


def get_name(field):

    if hasattr(field, "name"):
        return field.name
    if isinstance(field, dict):
        return field.get("name", "<unknown>")
    if isinstance(field, str):
        return field
    return "<unknown>"


def containers_passed_through_section(section_id: int):
    try:

        locs = fetch_localizations_by_section(section_id)
        containers = {c.containerId: get_name(c.containerName) for c in fetch_containers()}
        sections = {s.sectionId: get_name(s.name) for s in fetch_sections()}
        section_name = sections.get(section_id, f"Section {section_id}")

        container_dates = defaultdict(list)
        for loc in locs:
            container_dates[loc.containerId].append(loc.datetime)

        if not container_dates:
            print(f"No containers have passed through section '{section_name}'.")
            return

        print(f"Containers that have passed through section '{section_name}':")
        for cid, dates in container_dates.items():
            name = containers.get(cid, f"ID {cid}")
            print(f"- Container '{name}' (ID {cid}) on dates:")
            for dt in dates:
                print(f"    {dt}")

    except requests.RequestException as e:
        print(f"Error fetching data: {e}")
    except Exception as e:
        print(f"Unexpected error: {e}")
